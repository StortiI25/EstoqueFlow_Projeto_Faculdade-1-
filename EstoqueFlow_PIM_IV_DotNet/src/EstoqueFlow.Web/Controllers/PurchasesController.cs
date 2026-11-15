using EstoqueFlow.Web.Data;
using EstoqueFlow.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

public class PurchasesController(EstoqueFlowContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var orders = await db.PurchaseOrders.Include(x => x.Supplier).Include(x => x.Product)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();
        return View(orders);
    }

    public async Task<IActionResult> Create()
    {
        await LoadOptions();
        return View(new PurchaseOrder());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PurchaseOrder order)
    {
        if (!await db.Suppliers.AnyAsync(x => x.Id == order.SupplierId))
            ModelState.AddModelError(nameof(order.SupplierId), "Selecione um fornecedor válido.");
        if (!await db.Products.AnyAsync(x => x.Id == order.ProductId))
            ModelState.AddModelError(nameof(order.ProductId), "Selecione um produto válido.");
        if (!ModelState.IsValid) { await LoadOptions(); return View(order); }
        order.Status = "Pendente";
        db.PurchaseOrders.Add(order);
        await db.SaveChangesAsync();
        TempData["Success"] = "Pedido de compra criado. Receba o pedido quando a mercadoria chegar.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Receive(int id)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var order = await db.PurchaseOrders.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound();
        if (order.Status != "Pendente" || order.Product is null)
        {
            TempData["Error"] = "Este pedido já foi recebido ou não está disponível.";
            return RedirectToAction(nameof(Index));
        }

        order.Product.Quantity += order.Quantity;
        order.Status = "Recebido";
        order.ReceivedAt = DateTime.UtcNow;
        db.StockMovements.Add(new StockMovement
        {
            ProductId = order.ProductId,
            Type = "ENTRY",
            Quantity = order.Quantity,
            Notes = $"Recebimento do pedido de compra #{order.Id}"
        });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["Success"] = $"Pedido #{order.Id} recebido. Estoque atualizado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadOptions()
    {
        ViewBag.Suppliers = await db.Suppliers.OrderBy(x => x.Name).ToListAsync();
        ViewBag.Products = await db.Products.OrderBy(x => x.Name).ToListAsync();
    }
}
