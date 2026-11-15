using System.Security.Claims;
using EstoqueFlow.Web.Data;
using EstoqueFlow.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

[ApiController]
[Route("api/mobile")]
public class MobileApiController(
    EstoqueFlowContext db,
    IPasswordHasher<AppUser> passwordHasher) : ControllerBase
{
    public record LoginRequest(string Email, string Password);

    [AllowAnonymous, HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok", application = "EstoqueFlow API" });

    [AllowAnonymous, HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrEmpty(input.Password))
            return BadRequest(new { message = "Informe e-mail e senha." });

        var email = input.Email.Trim().ToUpperInvariant();
        var user = await db.AppUsers.FirstOrDefaultAsync(x => x.NormalizedEmail == email);
        if (user is null || !user.IsActive ||
            passwordHasher.VerifyHashedPassword(user, user.PasswordHash, input.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "E-mail ou senha inválidos, ou usuário inativo." });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity), new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });
        return Ok(new { message = "Login realizado.", name = user.FullName, role = user.Role });
    }

    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Sessão encerrada." });
    }
}
