using EstoqueFlow.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

public class HomeController(EstoqueFlowContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.ProductCount = await db.Products.CountAsync();
        ViewBag.LowStockCount = await db.Products.CountAsync(p => p.Quantity <= p.MinimumStock);
        ViewBag.MovementCount = await db.StockMovements.CountAsync();
        ViewBag.SupplierCount = await db.Suppliers.CountAsync();
        ViewBag.CustomerCount = await db.Customers.CountAsync();
        ViewBag.PendingPurchases = await db.PurchaseOrders.CountAsync(x => x.Status == "Pendente");
        ViewBag.PendingSales = await db.SalesOrders.CountAsync(x => x.Status == "Pendente");
        ViewBag.CompletedSales = await db.SalesOrders.CountAsync(x => x.Status == "Concluída");
        return View();
    }
}
