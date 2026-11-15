using EstoqueFlow.Web.Data;
using EstoqueFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

public class StockController(EstoqueFlowContext db, StockService stock) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.Products = await db.Products.OrderBy(p => p.Name).ToListAsync();
        ViewBag.Movements = await db.StockMovements
            .Include(m => m.Product)
            .OrderByDescending(m => m.CreatedAt)
            .Take(100).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int productId, int quantity, string type, string notes)
    {
        var result = await stock.RegisterAsync(productId, quantity, type, notes);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
