using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPortfolioBackend.Dtos;

namespace MyPortfolioBackend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(AccountDto dto)
    {
        var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        var rolResult = await _userManager.AddToRoleAsync(user, "Gast");

        if (!rolResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return BadRequest(rolResult.Errors);
        }

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(AccountDto dto)
    {
        // Gebruiker opzoeken op e-mailadres en dan inloggen (zet de authenticatiecookie).
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _signInManager.PasswordSignInAsync(user, dto.Password, false, false);

        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        return Ok();
    }

    // Laat zien wie er is ingelogd en met welke rollen (handig voor de latere React-app).
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var rollen = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        return Ok(new { email = User.Identity?.Name, rollen });
    }
}
