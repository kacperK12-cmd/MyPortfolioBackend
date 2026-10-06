using Microsoft.EntityFrameworkCore;
using MyPortfolioBackend.Entities;

namespace MyPortfolioBackend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<Blogpost> Blogposts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=MyPortfolioDb;User Id=sa;Password=JouwSterkeWachtwoord123!;TrustServerCertificate=True;");
        }
    }
}