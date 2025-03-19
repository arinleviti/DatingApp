using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class MessageHub(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper, IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        /* Extract the user query parameter from the request URL: */
        var otherUser = httpContext?.Request.Query["user"];

        if (Context.User == null || string.IsNullOrEmpty(otherUser)) throw new Exception("Cannot join group");
        var groupName = GetGroupName(Context.User.GetUsername(), otherUser);

        /* Add the user to a SignalR group. Groups in SignalR allow users to join a specific chat room.
        This ensures that only users in the same private chat group receive messages. */
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToGroup(groupName);

        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        /* This fetches previous messages between the two users from the message repository (likely a database or cache). */
        var message = await messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser!);

        /* Sends the message thread to everyone in the group (likely just two users). */
        await Clients.Caller.SendAsync("ReceiveMessageThread", message);
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var group = await RemoveFromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
         var username = Context.User?.GetUsername() ?? throw new Exception("Could not get user");
        if (username == createMessageDto.RecipientUsername.ToLower())
        throw new HubException("You can't message yourself");

        var sender = await userRepository.GetUserByUsernameAsync(username);
        var recipient = await userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null) 
        throw new HubException("Cannot send message at this time");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await messageRepository.GetMessageGroup(groupName);
        if(group != null && group.Connections.Any(x => x.UserName == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
            if (connections != null && connections?.Count != null)
            {
                /* presenceHub: This is likely your Hub class instance.
                .Clients: Accesses the clients connected to this hub.
                .Clients(connections): Sends a message to specific clients whose connection IDs are stored in connections (a collection of connection IDs).
                .SendAsync("NewMessageReceived", ...);: Sends an asynchronous message with the event name "NewMessageReceived", so clients listening for this event will receive it. */
                await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", 
                new {username = sender.UserName, knownAs = sender.KnownAs});
            }
        }
        messageRepository.AddMessage(message);

        if (await messageRepository.SaveAllAsync())
        {
           
            await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
        }
    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var username = Context.User?.GetUsername() ?? throw new Exception("Cannot get username");
        var group = await messageRepository.GetMessageGroup(groupName);
        var connection = new Connection{ConnectionId = Context.ConnectionId, UserName = username};

        if (group == null)
        {
            group = new Group{Name = groupName};
            messageRepository.AddGroup(group);
        }
        group.Connections.Add(connection);

        if (await messageRepository.SaveAllAsync()) return group;
        throw new HubException("Failed to join group");
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
        var group = await messageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        /* var connection = await messageRepository.GetConnection(Context.ConnectionId); */
        if (connection != null && group!= null)
        {
            messageRepository.RemoveConnection(connection);
            if (await messageRepository.SaveAllAsync())
            {
                return group;
            };
        }
        throw new Exception ("Failed to remove from group");
    }

    private string GetGroupName(string caller, string? other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}
