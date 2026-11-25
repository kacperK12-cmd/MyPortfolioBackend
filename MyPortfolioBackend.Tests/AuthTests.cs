using System.Net;
using System.Net.Http.Json;
using MyPortfolioBackend.Dtos;

namespace MyPortfolioBackend.Tests;

public class AuthTests : IClassFixture<TestFactory>
{
    private readonly TestFactory _factory;

    public AuthTests(TestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_NieuweGebruiker_Geeft200EnRolGast()
    {
        // Arrange
        var client = _factory.CreateClient();
        var account = new AccountDto { Email = "nieuw@test.nl", Password = TestFactory.Wachtwoord };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", account);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // In de database heeft de gebruiker alleen de rol Gast.
        using var db = _factory.MaakDb();
        var user = db.Users.Single(u => u.Email == "nieuw@test.nl");
        var rollen = (from userRole in db.UserRoles
                      join role in db.Roles on userRole.RoleId equals role.Id
                      where userRole.UserId == user.Id
                      select role.Name).ToList();
        Assert.Equal("Gast", Assert.Single(rollen));
    }

    [Fact]
    public async Task Register_ZwakWachtwoord_Geeft400()
    {
        // Arrange
        var client = _factory.CreateClient();
        var account = new AccountDto { Email = "zwak@test.nl", Password = "abc" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", account);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_JuisteGegevens_Geeft200EnCookie()
    {
        // Arrange
        var client = _factory.CreateClient();
        var account = new AccountDto { Email = "editor@test.nl", Password = TestFactory.Wachtwoord };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", account);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // De API geeft een Identity-authenticatiecookie terug.
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies!, c => c.StartsWith(".AspNetCore.Identity.Application="));
    }

    [Fact]
    public async Task Login_FoutWachtwoord_Geeft401()
    {
        // Arrange
        var client = _factory.CreateClient();
        var account = new AccountDto { Email = "editor@test.nl", Password = "Fout123!" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", account);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private record MeAntwoord(string Email, List<string> Rollen);

    [Fact]
    public async Task Me_AlsEditor_Geeft200MetRolEditor()
    {
        // Arrange
        var client = await _factory.LoginAlsAsync("editor@test.nl");

        // Act
        var response = await client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var me = await response.Content.ReadFromJsonAsync<MeAntwoord>();
        Assert.Equal("editor@test.nl", me!.Email);
        Assert.Contains("Editor", me.Rollen);
    }

    [Fact]
    public async Task Me_ZonderLogin_Geeft401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
