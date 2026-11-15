using EstoqueFlow.Web.Data;
using EstoqueFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class SettingsController(EstoqueFlowContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var settings = await db.SystemSettings.FirstOrDefaultAsync(x => x.Id == 1)
            ?? new SystemSettings { Id = 1 };
        return View(settings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SystemSettings input)
    {
        if (!ModelState.IsValid) return View(input);
        var settings = await db.SystemSettings.FirstOrDefaultAsync(x => x.Id == 1);
        if (settings is null)
        {
            settings = new SystemSettings { Id = 1 };
            db.SystemSettings.Add(settings);
        }

        settings.CompanyName = input.CompanyName.Trim();
        settings.Document = input.Document.Trim();
        settings.Email = input.Email.Trim();
        settings.Phone = input.Phone.Trim();
        settings.Address = input.Address.Trim();
        settings.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Success"] = "Configurações da empresa salvas com sucesso.";
        return RedirectToAction(nameof(Index));
    }
}
