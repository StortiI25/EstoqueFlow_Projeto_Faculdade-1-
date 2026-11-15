using EstoqueFlow.Web.Data;
using EstoqueFlow.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

public class SalesController(EstoqueFlowContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var orders = await db.SalesOrders.Include(x => x.Customer).Include(x => x.Product)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();
        return View(orders);
    }

    public async Task<IActionResult> Create()
    {
        await LoadOptions();
        return View(new SalesOrder());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SalesOrder order)
    {
        if (!await db.Customers.AnyAsync(x => x.Id == order.CustomerId))
            ModelState.AddModelError(nameof(order.CustomerId), "Selecione um cliente válido.");
        if (!await db.Products.AnyAsync(x => x.Id == order.ProductId))
            ModelState.AddModelError(nameof(order.ProductId), "Selecione um produto válido.");
        if (!ModelState.IsValid) { await LoadOptions(); return View(order); }

        var product = await db.Products.FindAsync(order.ProductId);
        if (product is null) return NotFound();

        order.Status = "Pendente";
        // O pedido fica pendente; o saldo só é baixado ao confirmar a venda.
        db.SalesOrders.Add(order);
        await db.SaveChangesAsync();
        TempData["Success"] = "Pedido de venda criado. Confirme a venda após validar os dados.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var order = await db.SalesOrders.Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound();

        if (order.Status != "Pendente" || order.Product is null)
        {
            TempData["Error"] = "Este pedido já foi concluído ou não está disponível.";
            return RedirectToAction(nameof(Index));
        }

        if (order.Product.Quantity < order.Quantity)
        {
            TempData["Error"] = $"Estoque insuficiente. Saldo disponível: {order.Product.Quantity}.";
            return RedirectToAction(nameof(Index));
        }

        order.Product.Quantity -= order.Quantity;
        order.Status = "Concluída";
        order.CompletedAt = DateTime.UtcNow;
        db.StockMovements.Add(new StockMovement
        {
            ProductId = order.ProductId,
            Type = "EXIT",
            Quantity = order.Quantity,
            Notes = $"Saída referente à venda #{order.Id}"
        });

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["Success"] = $"Venda #{order.Id} concluída. Estoque atualizado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadOptions()
    {
        ViewBag.Customers = await db.Customers.OrderBy(x => x.Name).ToListAsync();
        ViewBag.Products = await db.Products.OrderBy(x => x.Name).ToListAsync();
    }
}
