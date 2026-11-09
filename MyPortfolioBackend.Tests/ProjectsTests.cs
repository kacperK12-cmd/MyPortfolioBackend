using System.Net;
using System.Net.Http.Json;
using MyPortfolioBackend.Dtos;
using MyPortfolioBackend.Entities;

namespace MyPortfolioBackend.Tests;

public class ProjectsTests : IClassFixture<TestFactory>
{
    private readonly TestFactory _factory;

    public ProjectsTests(TestFactory factory)
    {
        _factory = factory;
    }

    // Zet een project in de testdatabase.
    private Project VoegProjectToe()
    {
        using var db = _factory.MaakDb();
        var project = new Project
        {
            Titel = "Bestaand project",
            Beschrijving = "Test",
            Categorie = "Web",
            GitHubUrl = "https://github.com/test",
            Datum = new DateTime(2026, 1, 1)
        };
        db.Projects.Add(project);
        db.SaveChanges();
        return project;
    }

    private static CreateProjectDto NieuwProject() => new()
    {
        Titel = "Nieuw project",
        Beschrijving = "Beschrijving",
        Categorie = "Backend",
        GitHubUrl = "https://github.com/nieuw",
        Datum = new DateTime(2026, 2, 1)
    };

    // ---------- GET ----------

    [Fact]
    public async Task GetAll_ZonderLogin_Geeft200MetProjecten()
    {
        // Arrange
        var project = VoegProjectToe();
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/projects");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var projecten = await response.Content.ReadFromJsonAsync<List<ProjectDto>>();
        Assert.Contains(projecten!, p => p.Id == project.Id);
    }

    [Fact]
    public async Task GetById_BestaandId_Geeft200MetJuisteProject()
    {
        // Arrange
        var project = VoegProjectToe();
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/projects/{project.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resultaat = await response.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.Equal("Bestaand project", resultaat!.Titel);
    }

    [Fact]
    public async Task GetById_OnbekendId_Geeft404()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/projects/99999");

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
        var aantalVoor = db.Projects.Count();

        // Act
        var response = await client.PostAsJsonAsync("/api/projects", NieuwProject());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(aantalVoor, db.Projects.Count());
    }

    [Fact]
    public async Task Post_AlsGast_Geeft403EnSlaatNietsOp()
    {
        // Arrange
        var client = await _factory.LoginAlsGastAsync();
        using var db = _factory.MaakDb();
        var aantalVoor = db.Projects.Count();

        // Act
        var response = await client.PostAsJsonAsync("/api/projects", NieuwProject());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(aantalVoor, db.Projects.Count());
    }

    [Theory]
    [InlineData("editor@test.nl")]
    [InlineData("admin@test.nl")]
    public async Task Post_AlsEditorOfAdmin_Geeft201EnSlaatOp(string email)
    {
        // Arrange
        var client = await _factory.LoginAlsAsync(email);

        // Act
        var response = await client.PostAsJsonAsync("/api/projects", NieuwProject());

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var aangemaakt = await response.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.Equal("Nieuw project", aangemaakt!.Titel);

        using var db = _factory.MaakDb();
        Assert.NotNull(db.Projects.Find(aangemaakt.Id)); // staat echt in de database
    }

    [Fact]
    public async Task Post_OngeldigeInvoer_Geeft400()
    {
        // Arrange
        var client = await _factory.LoginAlsAsync("editor@test.nl");
        var ongeldig = NieuwProject();
        ongeldig.Titel = ""; // Titel is verplicht

        // Act
        var response = await client.PostAsJsonAsync("/api/projects", ongeldig);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- PUT ----------

    [Fact]
    public async Task Put_AlsEditor_Geeft200EnWijzigtProject()
    {
        // Arrange
        var project = VoegProjectToe();
        var client = await _factory.LoginAlsAsync("editor@test.nl");
        var wijziging = NieuwProject();
        wijziging.Titel = "Gewijzigde titel";

        // Act
        var response = await client.PutAsJsonAsync($"/api/projects/{project.Id}", wijziging);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var db = _factory.MaakDb();
        Assert.Equal("Gewijzigde titel", db.Projects.Find(project.Id)!.Titel); // echt gewijzigd
    }

    [Fact]
    public async Task Put_OnbekendId_Geeft404()
    {
        // Arrange
        var client = await _factory.LoginAlsAsync("editor@test.nl");

        // Act
        var response = await client.PutAsJsonAsync("/api/projects/99999", NieuwProject());

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- DELETE ----------

    [Fact]
    public async Task Delete_AlsAdmin_Geeft204EnVerwijdertProject()
    {
        // Arrange
        var project = VoegProjectToe();
        var client = await _factory.LoginAlsAsync("admin@test.nl");

        // Act
        var response = await client.DeleteAsync($"/api/projects/{project.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var db = _factory.MaakDb();
        Assert.Null(db.Projects.Find(project.Id)); // echt verwijderd
    }

    [Fact]
    public async Task Delete_ZonderLogin_Geeft401EnVerwijdertNiets()
    {
        // Arrange
        var project = VoegProjectToe();
        var client = _factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/projects/{project.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var db = _factory.MaakDb();
        Assert.NotNull(db.Projects.Find(project.Id)); // bestaat nog
    }
}
