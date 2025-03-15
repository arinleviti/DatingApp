using System;
using Microsoft.AspNetCore.Identity;

namespace API.Entities;
//joint table between role and user
public class AppUserRole : IdentityUserRole<int>
{
    public AppUser User { get; set; } = null!;
    public AppRole Role { get; set; } = null!;
}
