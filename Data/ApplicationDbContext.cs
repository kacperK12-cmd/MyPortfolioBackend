using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyPortfolioBackend.Entities;

namespace MyPortfolioBackend.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<Blogpost> Blogposts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "1", Name = "Gast", NormalizedName = "GAST", ConcurrencyStamp = "1" },
            new IdentityRole { Id = "2", Name = "Editor", NormalizedName = "EDITOR", ConcurrencyStamp = "2" },
            new IdentityRole { Id = "3", Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "3" });
    }
}