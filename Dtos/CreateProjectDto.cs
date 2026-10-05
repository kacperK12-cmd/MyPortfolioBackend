namespace MyPortfolioBackend.Dtos;

public class CreateProjectDto
{
    public string Titel { get; set; } = string.Empty;
    public string Beschrijving { get; set; } = string.Empty;
    public string Categorie { get; set; } = string.Empty;
    public string GitHubUrl { get; set; } = string.Empty;
    public DateTime Datum { get; set; }
}