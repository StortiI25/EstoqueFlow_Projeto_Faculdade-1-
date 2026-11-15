using EstoqueFlow.Web.Data;
using EstoqueFlow.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

public class ProductsController(EstoqueFlowContext db) : Controller
{
    public async Task<IActionResult> Index()
        => View(await db.Products.OrderBy(p => p.Name).ToListAsync());

    public IActionResult Create() => View(new Product());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid) return View(product);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await db.Products.FindAsync(id);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product input)
    {
        if (id != input.Id) return BadRequest();
        if (!ModelState.IsValid) return View(input);

        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.Name = input.Name;
        product.Category = input.Category;
        product.Price = input.Price;
        product.MinimumStock = input.MinimumStock;
        // O saldo não é editado nesta tela: deve mudar por movimentação registrada.
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
