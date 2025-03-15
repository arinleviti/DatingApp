
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
using Microsoft.AspNetCore.Identity;

public class AccountController (UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper) : BaseClassController
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register (RegisterDTO registerDTO)
        {
            if (await UserExists(registerDTO.Username)) return BadRequest("Username is taken");
            

            using var hmac = new HMACSHA512();

            var user= mapper.Map<AppUser>(registerDTO);
            user.UserName = registerDTO.Username.ToLower();

            var result = await userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);
/*             user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
            user.PasswordSalt = hmac.Key; */

           /*  context.Users.Add(user); */

            //We only use await when there’s a real asynchronous operation—like saving changes to the database, 
            // which may take an unknown amount of time.
            /* await context.SaveChangesAsync(); */

            return new UserDto
            {
                Username = user.UserName,
                Token = await tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login (LoginDto loginDTO)
        {
            var user= await userManager.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync( x => x.NormalizedUserName == loginDTO.Username.ToUpper());
            if (user == null || user.UserName == null) return Unauthorized("Invalid username");

            var result = await userManager.CheckPasswordAsync(user, loginDTO.Password);

            if (!result) return Unauthorized();
        /* FOllowing code no longer needed as Identity */

   /*          using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            } */
            //return user;
            return new UserDto
            {
                Username = user.UserName,
                Token = await tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        private async Task<bool> UserExists(string username)
        {
            return await userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
        }
    }

