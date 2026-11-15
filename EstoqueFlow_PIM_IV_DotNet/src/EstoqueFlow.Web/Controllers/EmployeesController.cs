using System.Security.Claims;
using EstoqueFlow.Web.Data;
using EstoqueFlow.Web.Models;
using EstoqueFlow.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class EmployeesController(
    EstoqueFlowContext db,
    IPasswordHasher<AppUser> passwordHasher) : Controller
{
    public async Task<IActionResult> Index() =>
        View(await db.AppUsers.OrderBy(u => u.FullName).ToListAsync());

    public IActionResult Create() => View(new EmployeeCreateViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeCreateViewModel input)
    {
        ValidateRole(input.Role);
        ValidatePassword(input.Password, nameof(input.Password));
        var normalizedEmail = NormalizeEmail(input.Email);
        if (await db.AppUsers.AnyAsync(u => u.NormalizedEmail == normalizedEmail))
            ModelState.AddModelError(nameof(input.Email), "Já existe um usuário com este e-mail.");
        if (!ModelState.IsValid) return View(input);

        var employee = new AppUser
        {
            FullName = input.FullName.Trim(),
            Email = input.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            Phone = input.Phone.Trim(),
            Role = input.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        employee.PasswordHash = passwordHasher.HashPassword(employee, input.Password);
        db.AppUsers.Add(employee);
        await db.SaveChangesAsync();
        TempData["Success"] = "Funcionário cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await db.AppUsers.FindAsync(id);
        if (user is null) return NotFound();
        return View(new EmployeeEditViewModel
        {
            Id = user.Id, FullName = user.FullName, Email = user.Email,
            Phone = user.Phone, Role = user.Role, IsActive = user.IsActive
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmployeeEditViewModel input)
    {
        if (id != input.Id) return BadRequest();
        ValidateRole(input.Role);
        var user = await db.AppUsers.FindAsync(id);
        if (user is null) return NotFound();

        var normalizedEmail = NormalizeEmail(input.Email);
        if (await db.AppUsers.AnyAsync(u => u.Id != id && u.NormalizedEmail == normalizedEmail))
            ModelState.AddModelError(nameof(input.Email), "Já existe um usuário com este e-mail.");

        var currentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isSelf = int.TryParse(currentId, out var loggedId) && loggedId == id;
        var isRemovingAdmin = user.Role == "Administrador" &&
                              (input.Role != "Administrador" || !input.IsActive);
        if (isSelf && isRemovingAdmin)
            ModelState.AddModelError(string.Empty, "Você não pode remover seu próprio acesso de administrador.");

        if (isRemovingAdmin)
        {
            var activeAdmins = await db.AppUsers.CountAsync(u => u.IsActive && u.Role == "Administrador");
            if (activeAdmins <= 1)
                ModelState.AddModelError(string.Empty, "O sistema precisa manter pelo menos um administrador ativo.");
        }

        if (!ModelState.IsValid) return View(input);

        user.FullName = input.FullName.Trim();
        user.Email = input.Email.Trim();
        user.NormalizedEmail = normalizedEmail;
        user.Phone = input.Phone.Trim();
        user.Role = input.Role;
        user.IsActive = input.IsActive;
        await db.SaveChangesAsync();
        TempData["Success"] = "Cadastro do funcionário atualizado.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidateRole(string role)
    {
        if (role is not ("Administrador" or "Funcionario"))
            ModelState.AddModelError("Role", "Selecione um perfil de acesso válido.");
    }

    private void ValidatePassword(string password, string field)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 10 ||
            !password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
            ModelState.AddModelError(field,
                "Use pelo menos 10 caracteres, incluindo letras maiúsculas, minúsculas e números.");
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
