using System;
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;
namespace API.Extensions;


public static class ApplicationServiceExtentions
{
    //this extends the IServiceCollection
    //The this IServiceCollection services parameter means that 
    // this method can be called directly on any instance of IServiceCollection, just like a regular method on that object.
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

        // Add logging services (this will log to the console).
        services.AddLogging(options =>
        {
            options.AddConsole(); // Logs to the console
        });


        //It registers your DataContext with dependency injection.
        //You pass a configuration function that tells ASP.NET Core how to set it up.
        //You use the UseSqlite method to tell Entity Framework Core to use SQLite as the database provider.
        //Once the DbContextOptionsBuilder has finished building the options, it produces an instance of DbContextOptions, 
        // which is then passed to the DbContext constructor.
        services.AddDbContext<DataContext>(opt =>
        {
            opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });
        services.AddCors();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILikesRepository, LikesRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<LogUserActivity>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
        
        return services;
    }
}
