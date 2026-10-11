using Microsoft.EntityFrameworkCore;
using MyPortfolioBackend.Data;
using MyPortfolioBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Connectionstring ophalen uit configuratie.
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connectionstring 'DefaultConnection' ontbreekt.");
        
// DbContext koppelen aan SQL Server.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Service beschikbaar maken via dependency injection.
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<BlogpostService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();