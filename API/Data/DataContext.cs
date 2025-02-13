using API.Entities;
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
public class DataContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
}