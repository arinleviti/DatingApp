using Microsoft.AspNetCore.Mvc;
using API.Entities;
using API.Data;
using Microsoft.EntityFrameworkCore;
using API.Controllers;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using AutoMapper;
using API.DTOs;
using System.Security.Claims;
using API.Extensions;
using API.Services;
using API.Helpers;
namespace API;

[Authorize]
public class UsersController : BaseClassController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;
    public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _photoService = photoService;
    }
    

    [HttpGet]
    public async Task< ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UsersParams userParams)
    {
        userParams.CurrentUser = User.GetUsername();
        
        var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(users);
        //you need to use Ok(users) instead of simply returning users because the Ok method is part of the ASP.NET Core's IActionResult return type, 
        // which allows you to control the HTTP response and return additional information, such as the status code, headers, and the actual data.
        
        /* var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users); */
        
        return Ok(users);
    }


    [HttpGet("{username}")]  // /api/users/3
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await _unitOfWork.UserRepository.GetMemberAsync(username);
        if (user == null) return NotFound();    

        return user;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser (MemberUpdateDto memberUpdateDto)
    {
/*         var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (username == null) return BadRequest("No username found in token"); */
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return BadRequest ("Could not find user");
        /* copies the properties from memberUpdateDto into user, updating the existing user object with the new values from memberUpdateDto. */
        _mapper.Map(memberUpdateDto, user);

        if (await _unitOfWork.Complete()) return NoContent();
        return BadRequest("Failed to update the user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return BadRequest("Cannot update user");

        var result = await _photoService.AddPhotoAsync(file);
        if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if (user.Photos.Count == 0) photo.IsMain = true;

        user.Photos.Add(photo);

        if (await _unitOfWork.Complete()) 
        return CreatedAtAction(nameof(GetUser), new {username = user.UserName}, _mapper.Map<PhotoDto>(photo));
        /* return _mapper.Map<PhotoDto>(photo); */
        return BadRequest("Problem adding photo");
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return BadRequest("Couldn't find user");

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if (photo == null || photo.IsMain) return BadRequest("Cannot use this as main photo");

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;

        if (await _unitOfWork.Complete()) return NoContent();
        return BadRequest("Failed to set main photo");
    }

    [HttpDelete("delete-photo/{photoId:int}")]

    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return BadRequest("Couldn't find user");

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if (photo == null) return NotFound();

        if (photo == null || photo.IsMain) return BadRequest("You cannot delete your main photo");

        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (await _unitOfWork.Complete()) return Ok();
        return BadRequest("Failed to delete the photo");
    }
}

