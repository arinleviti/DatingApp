using API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//check out the class in the Extensions folder called ApplicationServiceExtensions.cs.
//This class contains an extension method that adds services to the IServiceCollection.
builder.Services.AddApplicationServices(builder.Configuration);

//check out the class in the Extensions folder called IdentityServiceExtensions.cs.
//This class contains an extension method that adds services to the IServiceCollection.
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//app.UseAuthorization();
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200", "https://localhost:4200"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
