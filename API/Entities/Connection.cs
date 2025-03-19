using System;

namespace API.Entities;

public class Connection
{
    /* ...Id is considered a primary key by EF */
    public required string ConnectionId { get; set; }
    public required string UserName { get; set; }
}
