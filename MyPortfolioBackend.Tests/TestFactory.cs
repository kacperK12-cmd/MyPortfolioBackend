using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyPortfolioBackend.Data;
using MyPortfolioBackend.Dtos;

namespace MyPortfolioBackend.Tests;

// Start de echte API, maar met een aparte SQLite-database in het geheugen.
// Zo raken de tests de gewone database nooit.
public class TestFactory : WebApplicationFactory<Program>
{
    public const string Wachtwoord = "Test123!";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection)));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();

        // Testgebruikers met de rol Editor en Admin.
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        MaakGebruiker(userManager, "editor@test.nl", "Editor");
        MaakGebruiker(userManager, "admin@test.nl", "Admin");

        return host;
    }

    private static void MaakGebruiker(UserManager<IdentityUser> userManager, string email, string rol)
    {
        var user = new IdentityUser { UserName = email, Email = email };
        userManager.CreateAsync(user, Wachtwoord).Wait();
        userManager.AddToRoleAsync(user, rol).Wait();
    }

    // Directe toegang tot de testdatabase (voor Arrange en Assert).
    public ApplicationDbContext MaakDb()
    {
        _ = Services; // zorgt dat de API en de testdatabase gestart zijn

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;
        return new ApplicationDbContext(options);
    }

    // Client die is ingelogd als de gegeven gebruiker (cookie wordt bewaard).
    public async Task<HttpClient> LoginAlsAsync(string email)
    {
        var client = CreateClient();
        await client.PostAsJsonAsync("/api/auth/login", new AccountDto { Email = email, Password = Wachtwoord });
        return client;
    }

    // Registreert een nieuwe gebruiker (rol Gast) en logt in.
    public async Task<HttpClient> LoginAlsGastAsync()
    {
        var email = $"gast{Guid.NewGuid():N}@test.nl";
        var client = CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", new AccountDto { Email = email, Password = Wachtwoord });
        await client.PostAsJsonAsync("/api/auth/login", new AccountDto { Email = email, Password = Wachtwoord });
        return client;
    }
}
