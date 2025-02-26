using Microsoft.AspNetCore.Mvc;
using API.Entities;
using API.Data;
using Microsoft.EntityFrameworkCore;
using API.Controllers;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using AutoMapper;
using API.DTOs;
namespace API;

[Authorize]
public class UsersController : BaseClassController
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    [HttpGet]
    public async Task< ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var users = await _userRepository.GetMembersAsync();
        //you need to use Ok(users) instead of simply returning users because the Ok method is part of the ASP.NET Core's IActionResult return type, 
        // which allows you to control the HTTP response and return additional information, such as the status code, headers, and the actual data.
        
        /* var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users); */
        
        return Ok(users);
    }

    [HttpGet("{username}")]  // /api/users/3
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await _userRepository.GetMemberAsync(username);
        if (user == null) return NotFound();    

        return user;
    }
}

