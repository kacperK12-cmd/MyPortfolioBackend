using Microsoft.EntityFrameworkCore;
using MyPortfolioBackend.Data;
using MyPortfolioBackend.Dtos;
using MyPortfolioBackend.Entities;

namespace MyPortfolioBackend.Services;

public class BlogpostService
{
    private readonly ApplicationDbContext _context;

    public BlogpostService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BlogpostDto>> GetAllAsync()
    {
        var blogposts = await _context.Blogposts
            .AsNoTracking()
            .ToListAsync();

        return blogposts.Select(ToDto).ToList();
    }

    public async Task<BlogpostDto?> GetByIdAsync(int id)
    {
        var blogpost = await _context.Blogposts
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        return blogpost is null ? null : ToDto(blogpost);
    }

    public async Task<BlogpostDto> CreateAsync(CreateBlogpostDto dto)
    {
        var blogpost = new Blogpost
        {
            Titel = dto.Titel,
            Inhoud = dto.Inhoud,
            Publicatiedatum = dto.Publicatiedatum
        };

        _context.Blogposts.Add(blogpost);
        await _context.SaveChangesAsync();

        return ToDto(blogpost);
    }

    public async Task<BlogpostDto?> UpdateAsync(
        int id,
        CreateBlogpostDto dto)
    {
        var blogpost = await _context.Blogposts.FindAsync(id);

        if (blogpost is null)
        {
            return null;
        }

        blogpost.Titel = dto.Titel;
        blogpost.Inhoud = dto.Inhoud;
        blogpost.Publicatiedatum = dto.Publicatiedatum;

        await _context.SaveChangesAsync();

        return ToDto(blogpost);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var blogpost = await _context.Blogposts.FindAsync(id);

        if (blogpost is null)
        {
            return false;
        }

        _context.Blogposts.Remove(blogpost);
        await _context.SaveChangesAsync();

        return true;
    }

    private static BlogpostDto ToDto(Blogpost blogpost)
    {
        return new BlogpostDto
        {
            Id = blogpost.Id,
            Titel = blogpost.Titel,
            Inhoud = blogpost.Inhoud,
            Publicatiedatum = blogpost.Publicatiedatum
        };
    }
}