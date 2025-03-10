using System;

namespace API.Entities;

public class UserLike
{
    //does the liking
    public AppUser SourceUser { get; set; } = null!;
    public int SourceUserId { get; set; }
    //is being liked
    public AppUser TargetUser { get; set; } = null!;
    public int TargetUserId { get; set; }
}
