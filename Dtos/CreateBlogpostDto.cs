using System.ComponentModel.DataAnnotations;

namespace MyPortfolioBackend.Dtos;

public class CreateBlogpostDto
{
    [Required]
    public string Titel { get; set; } = string.Empty;
    public string Inhoud { get; set; } = string.Empty;
    public DateTime Publicatiedatum { get; set; }
}