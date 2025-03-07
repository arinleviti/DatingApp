using System;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
{
    public async Task<MemberDto?> GetMemberAsync(string username)
    {
        return await context.Users
        .Where (x=> x.UserName == username)
        .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
        .SingleOrDefaultAsync();
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UsersParams usersParams)
    {
       var query = context.Users.AsQueryable();
       /*  this uses AutoMapper's ProjectTo<T>() method to convert each User entity into a MemberDto. */
       ;
       query = query.Where(x => x.UserName != usersParams.CurrentUser); 
       if (usersParams.Gender != null)
       {
           query = query.Where(x => x.Gender == usersParams.Gender);
       }

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-usersParams.MaxAge - 1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-usersParams.MinAge));
        query = query.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);
        query = usersParams.OrderBy switch
        {
            "created" => query.OrderByDescending(x => x.Created),
            _ => query.OrderByDescending(x => x.LastActive)
        };
        return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(mapper.ConfigurationProvider), usersParams.PageNumber, usersParams.PageSize);

    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        return await context.Users
        .Include(x => x.Photos)
        .SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await context.Users
        .Include(x => x.Photos)
        .ToListAsync();
    }

    public async Task<bool> SaveAllAsync()
    {
        //this method returns an integer so as long as the number is greater than zero, something has been saved.
        return await context.SaveChangesAsync() >0 ;
    }

    //this method is pretty useless because it doesn't really update the database. EF tracks the entity anyway   
    public void Update(AppUser user)
    {
        context.Entry(user).State = EntityState.Modified;
    }
}
