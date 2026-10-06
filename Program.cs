using Microsoft.EntityFrameworkCore;
using MyPortfolioBackend.Data;

var builder = WebApplication.CreateBuilder(args);

// Connection string ophalen uit appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext koppelen aan SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

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