using Microsoft.EntityFrameworkCore;
using MyPortfolioBackend.Data;
using MyPortfolioBackend.Dtos;
using MyPortfolioBackend.Entities;

namespace MyPortfolioBackend.Services;

public class ProjectService
{
    private readonly ApplicationDbContext _context;

    public ProjectService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Alle projecten ophalen.
    public async Task<List<ProjectDto>> GetAllAsync()
    {
        var projects = await _context.Projects
            .AsNoTracking()
            .ToListAsync();

        return projects.Select(ToDto).ToList();
    }

    // Eén project ophalen. Null betekent: niet gevonden.
    public async Task<ProjectDto?> GetByIdAsync(int id)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return project is null ? null : ToDto(project);
    }

    // Een project aanmaken. De database genereert het Id.
    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto)
    {
        var project = new Project
        {
            Titel = dto.Titel,
            Beschrijving = dto.Beschrijving,
            Categorie = dto.Categorie,
            GitHubUrl = dto.GitHubUrl,
            Datum = dto.Datum
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return ToDto(project);
    }

    // Een project wijzigen. Null betekent: niet gevonden.
    public async Task<ProjectDto?> UpdateAsync(
        int id,
        CreateProjectDto dto)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project is null)
        {
            return null;
        }

        project.Titel = dto.Titel;
        project.Beschrijving = dto.Beschrijving;
        project.Categorie = dto.Categorie;
        project.GitHubUrl = dto.GitHubUrl;
        project.Datum = dto.Datum;

        await _context.SaveChangesAsync();

        return ToDto(project);
    }

    // Een project verwijderen. False betekent: niet gevonden.
    public async Task<bool> DeleteAsync(int id)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project is null)
        {
            return false;
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return true;
    }

    // Centrale omzetting van database-entiteit naar uitvoer-DTO.
    private static ProjectDto ToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Titel = project.Titel,
            Beschrijving = project.Beschrijving,
            Categorie = project.Categorie,
            GitHubUrl = project.GitHubUrl,
            Datum = project.Datum
        };
    }
}