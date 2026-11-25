using System.Net;
using System.Net.Http.Json;
using MyPortfolioBackend.Dtos;
using MyPortfolioBackend.Entities;

namespace MyPortfolioBackend.Tests;

public class BlogpostsTests : IClassFixture<TestFactory>
{
    private readonly TestFactory _factory;

    public BlogpostsTests(TestFactory factory)
    {
        _factory = factory;
    }

    // Zet een blogpost in de testdatabase.
    private Blogpost VoegBlogpostToe()
    {
        using var db = _factory.MaakDb();
        var blogpost = new Blogpost
        {
            Titel = "Bestaande blogpost",
            Inhoud = "Test",
            Publicatiedatum = new DateTime(2026, 1, 1)
        };
        db.Blogposts.Add(blogpost);
        db.SaveChanges();
        return blogpost;
    }

    private static CreateBlogpostDto NieuweBlogpost() => new()
    {
        Titel = "Nieuwe blogpost",
        Inhoud = "Inhoud",
        Publicatiedatum = new DateTime(2026, 2, 1)
    };

    // ---------- GET ----------

    [Fact]
    public async Task GetAll_ZonderLogin_Geeft200MetBlogposts()
    {
        // Arrange
        var blogpost = VoegBlogpostToe();
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/blogposts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var blogposts = await response.Content.ReadFromJsonAsync<List<BlogpostDto>>();
        Assert.Contains(blogposts!, b => b.Id == blogpost.Id);
    }

    [Fact]
    public async Task GetById_BestaandId_Geeft200MetJuisteBlogpost()
    {
        // Arrange
        var blogpost = VoegBlogpostToe();
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/blogposts/{blogpost.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resultaat = await response.Content.ReadFromJsonAsync<BlogpostDto>();
        Assert.Equal("Bestaande blogpost", resultaat!.Titel);
    }

    [Fact]
    public async Task GetById_OnbekendId_Geeft404()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/blogposts/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- POST ----------

    [Fact]
    public async Task Post_ZonderLogin_Geeft401EnSlaatNietsOp()
    {
        // Arrange
        var client = _factory.CreateClient();
        using var db = _factory.MaakDb();
        var aantalVoor = db.Blogposts.Count();

        // Act
        var response = await client.PostAsJsonAsync("/api/blogposts", NieuweBlogpost());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(aantalVoor, db.Blogposts.Count());
    }

    [Fact]
    public async Task Post_AlsGast_Geeft403EnSlaatNietsOp()
    {
        // Arrange
        var client = await _factory.LoginAlsGastAsync();
        using var db = _factory.MaakDb();
        var aantalVoor = db.Blogposts.Count();

        // Act
        var response = await client.PostAsJsonAsync("/api/blogposts", NieuweBlogpost());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(aantalVoor, db.Blogposts.Count());
    }

    [Theory]
    [InlineData("editor@test.nl")]
    [InlineData("admin@test.nl")]
    public async Task Post_AlsEditorOfAdmin_Geeft201EnSlaatOp(string email)
    {
        // Arrange
        var client = await _factory.LoginAlsAsync(email);

        // Act
        var response = await client.PostAsJsonAsync("/api/blogposts", NieuweBlogpost());

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var aangemaakt = await response.Content.ReadFromJsonAsync<BlogpostDto>();
        Assert.Equal("Nieuwe blogpost", aangemaakt!.Titel);

        using var db = _factory.MaakDb();
        Assert.NotNull(db.Blogposts.Find(aangemaakt.Id)); // staat echt in de database
    }

    [Fact]
    public async Task Post_OngeldigeInvoer_Geeft400()
    {
        // Arrange
        var client = await _factory.LoginAlsAsync("editor@test.nl");
        var ongeldig = NieuweBlogpost();
        ongeldig.Titel = ""; // Titel is verplicht

        // Act
        var response = await client.PostAsJsonAsync("/api/blogposts", ongeldig);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- PUT ----------

    [Theory]
    [InlineData("editor@test.nl")]
    [InlineData("admin@test.nl")]
    public async Task Put_AlsEditorOfAdmin_Geeft200EnWijzigtBlogpost(string email)
    {
        // Arrange
        var blogpost = VoegBlogpostToe();
        var client = await _factory.LoginAlsAsync(email);
        var wijziging = NieuweBlogpost();
        wijziging.Titel = "Gewijzigde titel";

        // Act
        var response = await client.PutAsJsonAsync($"/api/blogposts/{blogpost.Id}", wijziging);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resultaat = await response.Content.ReadFromJsonAsync<BlogpostDto>();
        Assert.Equal("Gewijzigde titel", resultaat!.Titel);

        using var db = _factory.MaakDb();
        Assert.Equal("Gewijzigde titel", db.Blogposts.Find(blogpost.Id)!.Titel); // echt gewijzigd
    }

    [Fact]
    public async Task Put_ZonderLogin_Geeft401EnWijzigtNiets()
    {
        // Arrange
        var blogpost = VoegBlogpostToe();
        var client = _factory.CreateClient();
        var wijziging = NieuweBlogpost();
        wijziging.Titel = "Mag niet";

        // Act
        var response = await client.PutAsJsonAsync($"/api/blogposts/{blogpost.Id}", wijziging);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var db = _factory.MaakDb();
        Assert.Equal("Bestaande blogpost", db.Blogposts.Find(blogpost.Id)!.Titel); // niet gewijzigd
    }

    [Fact]
    public async Task Put_AlsGast_Geeft403EnWijzigtNiets()
    {
        // Arrange
        var blogpost = VoegBlogpostToe();
        var client = await _factory.LoginAlsGastAsync();
        var wijziging = NieuweBlogpost();
        wijziging.Titel = "Mag niet";

        // Act
        var response = await client.PutAsJsonAsync($"/api/blogposts/{blogpost.Id}", wijziging);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using var db = _factory.MaakDb();
        Assert.Equal("Bestaande blogpost", db.Blogposts.Find(blogpost.Id)!.Titel); // niet gewijzigd
    }

    [Fact]
    public async Task Put_OnbekendId_Geeft404()
    {
        // Arrange
        var client = await _factory.LoginAlsAsync("editor@test.nl");

        // Act
        var response = await client.PutAsJsonAsync("/api/blogposts/99999", NieuweBlogpost());

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- DELETE ----------

    [Theory]
    [InlineData("editor@test.nl")]
    [InlineData("admin@test.nl")]
    public async Task Delete_AlsEditorOfAdmin_Geeft204EnVerwijdertBlogpost(string email)
    {
        // Arrange
        var blogpost = VoegBlogpostToe();
        var client = await _factory.LoginAlsAsync(email);

        // Act
        var response = await client.DeleteAsync($"/api/blogposts/{blogpost.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var db = _factory.MaakDb();
        Assert.Null(db.Blogposts.Find(blogpost.Id)); // echt verwijderd
    }

    [Fact]
    public async Task Delete_ZonderLogin_Geeft401EnVerwijdertNiets()
    {
        // Arrange
        var blogpost = VoegBlogpostToe();
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/blogposts/{blogpost.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var db = _factory.MaakDb();
        Assert.NotNull(db.Blogposts.Find(blogpost.Id)); // bestaat nog
    }

    [Fact]
    public async Task Delete_AlsGast_Geeft403EnVerwijdertNiets()
    {
        // Arrange
        var blogpost = VoegBlogpostToe();
        var client = await _factory.LoginAlsGastAsync();

        // Act
        var response = await client.DeleteAsync($"/api/blogposts/{blogpost.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using var db = _factory.MaakDb();
        Assert.NotNull(db.Blogposts.Find(blogpost.Id)); // bestaat nog
    }

    [Fact]
    public async Task Delete_OnbekendId_Geeft404()
    {
        // Arrange
        var client = await _factory.LoginAlsAsync("editor@test.nl");

        // Act
        var response = await client.DeleteAsync("/api/blogposts/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
