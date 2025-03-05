
namespace API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.Controllers;
using API.Entities;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using AutoMapper;

public class AccountController (DataContext context, ITokenService tokenService, IMapper mapper) : BaseClassController
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register (RegisterDTO registerDTO)
        {
            if (await UserExists(registerDTO.Username)) return BadRequest("Username is taken");
            

            using var hmac = new HMACSHA512();

            var user= mapper.Map<AppUser>(registerDTO);
            user.UserName = registerDTO.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
            user.PasswordSalt = hmac.Key;

            context.Users.Add(user);

            //We only use await when there’s a real asynchronous operation—like saving changes to the database, 
            // which may take an unknown amount of time.
            await context.SaveChangesAsync();

            return new UserDto
            {
                Username = user.UserName,
                Token = tokenService.CreateToken(user),
                KnownAs = user.KnownAs
            };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login (LoginDto loginDTO)
        {
            var user= await context.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync( x => x.UserName == loginDTO.Username.ToLower());
            if (user == null) return Unauthorized("Invalid username");
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }
            //return user;
            return new UserDto
            {
                Username = user.UserName,
                Token = tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs
            };
        }
        private async Task<bool> UserExists(string username)
        {
            return await context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower());
        }
    }

