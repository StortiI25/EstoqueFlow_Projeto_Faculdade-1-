using EstoqueFlow.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

public class ReportsController(EstoqueFlowContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var products = await db.Products.AsNoTracking().ToListAsync();
        var sales = await db.SalesOrders.AsNoTracking().Where(x => x.Status == "Concluída").ToListAsync();
        ViewBag.ProductCount = products.Count;
        ViewBag.LowStock = products.Where(p => p.Quantity <= p.MinimumStock).OrderBy(p => p.Name).ToList();
        // SQLite não traduz Sum(decimal) em SQL; somamos os valores já carregados.
        ViewBag.StockValue = products.Sum(p => p.Price * p.Quantity);
        ViewBag.Movements = await db.StockMovements.Include(m => m.Product)
            .OrderByDescending(m => m.CreatedAt).Take(50).ToListAsync();
        ViewBag.PurchaseCount = await db.PurchaseOrders.CountAsync();
        ViewBag.SalesCount = await db.SalesOrders.CountAsync();
        ViewBag.CompletedSalesValue = sales.Sum(x => x.Quantity * x.UnitPrice);
        return View();
    }
}
