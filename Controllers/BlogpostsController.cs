using Microsoft.AspNetCore.Mvc;
using MyPortfolioBackend.Dtos;
using MyPortfolioBackend.Services;

namespace MyPortfolioBackend.Controllers;

[ApiController]
[Route("api/blogposts")]
public class BlogpostsController : ControllerBase
{
    private readonly BlogpostService _service;

    public BlogpostsController(BlogpostService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<BlogpostDto>>> GetAll()
    {
        var blogposts = await _service.GetAllAsync();
        return Ok(blogposts);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BlogpostDto>> GetById(int id)
    {
        var blogpost = await _service.GetByIdAsync(id);

        if (blogpost is null)
        {
            return NotFound();
        }

        return Ok(blogpost);
    }

    [HttpPost]
    public async Task<ActionResult<BlogpostDto>> Create(
        CreateBlogpostDto dto)
    {
        var blogpost = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = blogpost.Id },
            blogpost);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BlogpostDto>> Update(
        int id,
        CreateBlogpostDto dto)
    {
        var blogpost = await _service.UpdateAsync(id, dto);

        if (blogpost is null)
        {
            return NotFound();
        }

        return Ok(blogpost);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}