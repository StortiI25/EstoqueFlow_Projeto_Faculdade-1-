using EstoqueFlow.Web.Data;
using EstoqueFlow.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

public class SuppliersController(EstoqueFlowContext db) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Suppliers.OrderBy(x => x.Name).ToListAsync());
    public IActionResult Create() => View(new Supplier());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier supplier)
    {
        if (!ModelState.IsValid) return View(supplier);
        db.Suppliers.Add(supplier); await db.SaveChangesAsync();
        TempData["Success"] = "Fornecedor cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await db.Suppliers.FindAsync(id);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Supplier input)
    {
        if (id != input.Id) return BadRequest();
        if (!ModelState.IsValid) return View(input);
        var item = await db.Suppliers.FindAsync(id);
        if (item is null) return NotFound();
        item.Name=input.Name; item.Document=input.Document; item.Email=input.Email;
        item.Phone=input.Phone; item.Address=input.Address;
        await db.SaveChangesAsync();
        TempData["Success"] = "Fornecedor atualizado.";
        return RedirectToAction(nameof(Index));
    }
}
