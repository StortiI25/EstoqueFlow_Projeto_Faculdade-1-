using EstoqueFlow.Web.Data;
using EstoqueFlow.Web.Models;
using EstoqueFlow.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

[AllowAnonymous]
public class SetupController(
    EstoqueFlowContext db,
    IPasswordHasher<AppUser> passwordHasher) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (await db.AppUsers.AnyAsync()) return RedirectToAction("Login", "Auth");
        return View(new FirstAdminViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(FirstAdminViewModel input)
    {
        if (await db.AppUsers.AnyAsync()) return RedirectToAction("Login", "Auth");
        ValidatePasswordComplexity(input.Password, nameof(input.Password));
        if (!ModelState.IsValid) return View(input);

        var normalizedEmail = NormalizeEmail(input.Email);
        if (await db.AppUsers.AnyAsync(u => u.NormalizedEmail == normalizedEmail))
        {
            ModelState.AddModelError(nameof(input.Email), "Este e-mail já está cadastrado.");
            return View(input);
        }

        var admin = new AppUser
        {
            FullName = input.FullName.Trim(),
            Email = input.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            Role = "Administrador",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, input.Password);
        db.AppUsers.Add(admin);
        await db.SaveChangesAsync();

        TempData["Success"] = "Administrador criado. Entre com seu e-mail e senha.";
        return RedirectToAction("Login", "Auth");
    }

    private void ValidatePasswordComplexity(string password, string field)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 10 ||
            !password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
        {
            ModelState.AddModelError(field,
                "Use pelo menos 10 caracteres, incluindo letras maiúsculas, minúsculas e números.");
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
