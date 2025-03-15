using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;
//acts as a bridge between your C# code and the database.
//It allows you to query and save instances of your entities.
//It also allows you to specify how your entities map to the database schema.
//It is a combination of the Unit Of Work and Repository patterns.
//It tracks changes that you made to your entities and persists them to the database.
//It allows you to execute raw SQL queries.
//It allows you to add, update, and remove entities from the database.
//EF Core reads these properties and figures out how to structure the database.
public class DataContext(DbContextOptions options) : IdentityDbContext<AppUser, AppRole, int,
 IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>(options)

/* We can remove this since IdentityDbContext comes with a DbSet for our users */
{
/*     public DbSet<AppUser> Users { get; set; } */
    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    /* The use of HasMany and WithOne is due to the fact that UserRoles is a collection, 
    while User in AppUserRole is a single object reference. */
        builder.Entity<AppUser>()
        .HasMany(ur => ur.UserRoles)
        /* The .WithOne(u => u.User) means that each AppUserRole instance can only have one associated AppUser.
         In other words, each AppUserRole instance will be linked to one user. */
        .WithOne(u => u.User)
        .HasForeignKey(ur => ur.UserId)
        .IsRequired();

         builder.Entity<AppRole>()
        .HasMany(ur => ur.UserRoles)
        .WithOne(u => u.Role)
        .HasForeignKey(ur => ur.RoleId)
        .IsRequired();

        /* Composite Key: In the UserLike class, SourceUserId and TargetUserId
         are combined to form a composite primary key. This means that both columns together uniquely identify each record in the UserLikes table. 
         Why?: A UserLike represents a relationship between two users (one liking the other), and we don't want a single user to like another user multiple times,
          hence using both user IDs as a composite key.*/
        builder.Entity<UserLike>()
            .HasKey(key => new { key.SourceUserId, key.TargetUserId });
        
        builder.Entity<UserLike>()
        /* establishes that each UserLike belongs to one SourceUser, who is the user doing the liking. */
            .HasOne(s => s.SourceUser)
            /* establishes that one AppUser (the SourceUser) can have many UserLike instances where they are the "liker." */
            .WithMany(l => l.LikedUsers)
            /* specifies that SourceUserId in the UserLike table is the foreign key that connects the UserLike to the AppUser table (the SourceUser). */
            .HasForeignKey(s => s.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>()
            /* establishes that each UserLike also belongs to one TargetUser, who is the user being liked. */
            .HasOne(s => s.TargetUser)
            /* establishes that one AppUser (the TargetUser) can have many UserLike instances where they are the "liked" user. */
            .WithMany(l => l.LikedByUsers)
            .HasForeignKey(s => s.TargetUserId)
            //on Sql Server use NoAction instead of Cascade
            .OnDelete(DeleteBehavior.Cascade);

/* For the Message entity, however, you're not creating a many-to-many relationship, but rather two one-to-many relationships: */
            builder.Entity<Message>()
            .HasOne(x => x.Recipient)
            .WithMany(x => x.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
            .HasOne(x => x.Sender)
            .WithMany(x => x.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);
            
    }
}