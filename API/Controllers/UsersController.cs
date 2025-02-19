using Microsoft.AspNetCore.Mvc;
using API.Entities;
using API.Data;
using Microsoft.EntityFrameworkCore;
using API.Controllers;
using Microsoft.AspNetCore.Authorization;
namespace API;

public class UsersController : BaseClassController
{
    private readonly DataContext _context;
    public UsersController(DataContext context)
    {
        _context = context;
    }
    
    [AllowAnonymous]
    [HttpGet]
    public async Task< ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        return users;
    }

    [Authorize]
    [HttpGet("{id:int}")]  // /api/users/3
    public async Task<ActionResult<AppUser>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();    

        return user;
    }
}

