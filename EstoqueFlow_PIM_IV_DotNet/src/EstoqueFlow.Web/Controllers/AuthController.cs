using System.Security.Claims;
using EstoqueFlow.Web.Data;
using EstoqueFlow.Web.Models;
using EstoqueFlow.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

public class AuthController(
    EstoqueFlowContext db,
    IPasswordHasher<AppUser> passwordHasher) : Controller
{
    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        if (!await db.AppUsers.AnyAsync()) return RedirectToAction("Index", "Setup");
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel input, string? returnUrl = null)
    {
        if (!await db.AppUsers.AnyAsync()) return RedirectToAction("Index", "Setup");
        if (!ModelState.IsValid)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(input);
        }

        var normalizedEmail = input.Email.Trim().ToUpperInvariant();
        var user = await db.AppUsers.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            return View(input);
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, input.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            return View(input);
        }
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, input.Password);
            await db.SaveChangesAsync();
        }

        await SignInUser(user, input.RememberMe);
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet, Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel input)
    {
        if (!ModelState.IsValid) return View(input);
        if (input.NewPassword.Length < 10 || !input.NewPassword.Any(char.IsUpper) ||
            !input.NewPassword.Any(char.IsLower) || !input.NewPassword.Any(char.IsDigit))
        {
            ModelState.AddModelError(nameof(input.NewPassword),
                "Use pelo menos 10 caracteres, incluindo letras maiúsculas, minúsculas e números.");
            return View(input);
        }

        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idValue, out var id)) return Challenge();
        var user = await db.AppUsers.FindAsync(id);
        if (user is null || !user.IsActive) return Challenge();

        var verify = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, input.CurrentPassword);
        if (verify == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(input.CurrentPassword), "A senha atual não confere.");
            return View(input);
        }

        user.PasswordHash = passwordHasher.HashPassword(user, input.NewPassword);
        await db.SaveChangesAsync();
        await SignInUser(user, isPersistent: false);
        TempData["Success"] = "Senha alterada com sucesso.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private async Task SignInUser(AppUser user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            principal, new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(isPersistent ? 24 : 8)
            });
    }
}
