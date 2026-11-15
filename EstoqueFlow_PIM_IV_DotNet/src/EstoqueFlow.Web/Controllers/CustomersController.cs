using EstoqueFlow.Web.Data;
using EstoqueFlow.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

public class CustomersController(EstoqueFlowContext db) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Customers.OrderBy(x => x.Name).ToListAsync());
    public IActionResult Create() => View(new Customer());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (!ModelState.IsValid) return View(customer);
        db.Customers.Add(customer); await db.SaveChangesAsync();
        TempData["Success"] = "Cliente cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await db.Customers.FindAsync(id);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Customer input)
    {
        if (id != input.Id) return BadRequest();
        if (!ModelState.IsValid) return View(input);
        var item = await db.Customers.FindAsync(id);
        if (item is null) return NotFound();
        item.Name=input.Name; item.Document=input.Document; item.Email=input.Email;
        item.Phone=input.Phone; item.Address=input.Address;
        await db.SaveChangesAsync();
        TempData["Success"] = "Cliente atualizado.";
        return RedirectToAction(nameof(Index));
    }
}
