using EstoqueFlow.Web.Data;
using EstoqueFlow.Web.Models;
using EstoqueFlow.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

[ApiController]
[Route("api/mobile/data")]
[Authorize]
public class MobileDataApiController(EstoqueFlowContext db, StockService stock) : ControllerBase
{
    public record ProductInput(string Name, string Category, decimal Price, int Quantity, int MinimumStock);
    public record ContactInput(string Name, string Document, string Email, string Phone, string Address);
    public record OrderInput(int PartnerId, int ProductId, int Quantity, decimal UnitPrice, string? Notes);
    public record MovementInput(int ProductId, int Quantity, string Type, string? Notes);
    public record SettingsInput(string CompanyName, string Document, string Email, string Phone, string Address);

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var products = await db.Products.AsNoTracking().ToListAsync();
        var sales = await db.SalesOrders.AsNoTracking().Where(x => x.Status == "Concluída").ToListAsync();
        return Ok(new {
            totalProducts = products.Count,
            lowStock = products.Count(p => p.Quantity <= p.MinimumStock),
            stockUnits = products.Sum(p => p.Quantity),
            stockValue = products.Sum(p => p.Price * p.Quantity),
            suppliers = await db.Suppliers.CountAsync(),
            customers = await db.Customers.CountAsync(),
            pendingPurchases = await db.PurchaseOrders.CountAsync(x => x.Status == "Pendente"),
            pendingSales = await db.SalesOrders.CountAsync(x => x.Status == "Pendente"),
            completedSalesValue = sales.Sum(x => x.Quantity * x.UnitPrice),
            recentMovements = await db.StockMovements.Include(x => x.Product).OrderByDescending(x => x.CreatedAt).Take(8)
                .Select(x => new { x.Id, product = x.Product!.Name, x.Type, x.Quantity, x.Notes, x.CreatedAt }).ToListAsync()
        });
    }

    [HttpGet("products")]
    public async Task<IActionResult> Products() => Ok(await db.Products.AsNoTracking().OrderBy(x => x.Name).ToListAsync());

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct(ProductInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || input.Price < 0 || input.Quantity < 0 || input.MinimumStock < 0)
            return BadRequest(new { message = "Confira nome, preço e quantidades." });
        var item = new Product { Name = input.Name.Trim(), Category = input.Category?.Trim() ?? "", Price = input.Price, Quantity = input.Quantity, MinimumStock = input.MinimumStock };
        db.Products.Add(item); await db.SaveChangesAsync(); return Ok(item);
    }

    [HttpPut("products/{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, ProductInput input)
    {
        var item = await db.Products.FindAsync(id); if (item is null) return NotFound();
        if (string.IsNullOrWhiteSpace(input.Name) || input.Price < 0 || input.Quantity < 0 || input.MinimumStock < 0) return BadRequest(new { message = "Dados inválidos." });
        item.Name = input.Name.Trim(); item.Category = input.Category?.Trim() ?? ""; item.Price = input.Price; item.Quantity = input.Quantity; item.MinimumStock = input.MinimumStock;
        await db.SaveChangesAsync(); return Ok(item);
    }

    [HttpDelete("products/{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var item = await db.Products.FindAsync(id); if (item is null) return NotFound();
        if (await db.StockMovements.AnyAsync(x => x.ProductId == id) || await db.PurchaseOrders.AnyAsync(x => x.ProductId == id) || await db.SalesOrders.AnyAsync(x => x.ProductId == id))
            return Conflict(new { message = "Produto possui histórico e não pode ser excluído." });
        db.Products.Remove(item); await db.SaveChangesAsync(); return Ok(new { message = "Produto excluído." });
    }

    [HttpGet("movements")]
    public async Task<IActionResult> Movements() => Ok(await db.StockMovements.AsNoTracking().Include(x => x.Product).OrderByDescending(x => x.CreatedAt).Take(200)
        .Select(x => new { x.Id, x.ProductId, product = x.Product!.Name, x.Type, x.Quantity, x.Notes, x.CreatedAt }).ToListAsync());

    [HttpPost("movements")]
    public async Task<IActionResult> CreateMovement(MovementInput input)
    {
        var result = await stock.RegisterAsync(input.ProductId, input.Quantity, input.Type, input.Notes ?? "");
        return result.Success ? Ok(new { message = result.Message }) : BadRequest(new { message = result.Message });
    }

    [HttpGet("suppliers")]
    public async Task<IActionResult> Suppliers() => Ok(await db.Suppliers.AsNoTracking().OrderBy(x => x.Name).ToListAsync());
    [HttpPost("suppliers")]
    public async Task<IActionResult> CreateSupplier(ContactInput input) => await SaveContact(null, input, true);
    [HttpPut("suppliers/{id:int}")]
    public async Task<IActionResult> UpdateSupplier(int id, ContactInput input) => await SaveContact(id, input, true);
    [HttpDelete("suppliers/{id:int}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var item = await db.Suppliers.FindAsync(id); if (item is null) return NotFound();
        if (await db.PurchaseOrders.AnyAsync(x => x.SupplierId == id)) return Conflict(new { message = "Fornecedor possui pedidos vinculados." });
        db.Suppliers.Remove(item); await db.SaveChangesAsync(); return Ok(new { message = "Fornecedor excluído." });
    }

    [HttpGet("customers")]
    public async Task<IActionResult> Customers() => Ok(await db.Customers.AsNoTracking().OrderBy(x => x.Name).ToListAsync());
    [HttpPost("customers")]
    public async Task<IActionResult> CreateCustomer(ContactInput input) => await SaveContact(null, input, false);
    [HttpPut("customers/{id:int}")]
    public async Task<IActionResult> UpdateCustomer(int id, ContactInput input) => await SaveContact(id, input, false);
    [HttpDelete("customers/{id:int}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var item = await db.Customers.FindAsync(id); if (item is null) return NotFound();
        if (await db.SalesOrders.AnyAsync(x => x.CustomerId == id)) return Conflict(new { message = "Cliente possui vendas vinculadas." });
        db.Customers.Remove(item); await db.SaveChangesAsync(); return Ok(new { message = "Cliente excluído." });
    }

    private async Task<IActionResult> SaveContact(int? id, ContactInput input, bool supplier)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) return BadRequest(new { message = "Informe o nome." });
        if (supplier)
        {
            var item = id.HasValue ? await db.Suppliers.FindAsync(id.Value) : new Supplier(); if (item is null) return NotFound();
            item.Name = input.Name.Trim(); item.Document = input.Document?.Trim() ?? ""; item.Email = input.Email?.Trim() ?? ""; item.Phone = input.Phone?.Trim() ?? ""; item.Address = input.Address?.Trim() ?? "";
            if (!id.HasValue) db.Suppliers.Add(item); await db.SaveChangesAsync(); return Ok(item);
        }
        else
        {
            var item = id.HasValue ? await db.Customers.FindAsync(id.Value) : new Customer(); if (item is null) return NotFound();
            item.Name = input.Name.Trim(); item.Document = input.Document?.Trim() ?? ""; item.Email = input.Email?.Trim() ?? ""; item.Phone = input.Phone?.Trim() ?? ""; item.Address = input.Address?.Trim() ?? "";
            if (!id.HasValue) db.Customers.Add(item); await db.SaveChangesAsync(); return Ok(item);
        }
    }

    [HttpGet("purchases")]
    public async Task<IActionResult> Purchases()
    {
        var rows = await db.PurchaseOrders.AsNoTracking().Include(x => x.Supplier).Include(x => x.Product).OrderByDescending(x => x.CreatedAt).ToListAsync();
        return Ok(rows.Select(x => new { x.Id, supplier = x.Supplier!.Name, product = x.Product!.Name, x.ProductId, x.SupplierId, x.Quantity, x.UnitPrice, total = x.Quantity * x.UnitPrice, x.Status, x.Notes, x.CreatedAt, x.ReceivedAt }));
    }

    [HttpPost("purchases")]
    public async Task<IActionResult> CreatePurchase(OrderInput input)
    {
        if (input.Quantity <= 0 || input.UnitPrice <= 0 || !await db.Suppliers.AnyAsync(x => x.Id == input.PartnerId) || !await db.Products.AnyAsync(x => x.Id == input.ProductId)) return BadRequest(new { message = "Confira fornecedor, produto, quantidade e preço." });
        var order = new PurchaseOrder { SupplierId = input.PartnerId, ProductId = input.ProductId, Quantity = input.Quantity, UnitPrice = input.UnitPrice, Notes = input.Notes ?? "", Status = "Pendente" };
        db.PurchaseOrders.Add(order); await db.SaveChangesAsync(); return Ok(new { message = "Pedido de compra criado.", id = order.Id });
    }

    [HttpPost("purchases/{id:int}/receive")]
    public async Task<IActionResult> ReceivePurchase(int id)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var order = await db.PurchaseOrders.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound(); if (order.Status != "Pendente" || order.Product is null) return Conflict(new { message = "Pedido já recebido ou indisponível." });
        order.Product.Quantity += order.Quantity; order.Status = "Recebido"; order.ReceivedAt = DateTime.UtcNow;
        db.StockMovements.Add(new StockMovement { ProductId = order.ProductId, Type = "ENTRY", Quantity = order.Quantity, Notes = $"Recebimento do pedido de compra #{order.Id}" });
        await db.SaveChangesAsync(); await tx.CommitAsync(); return Ok(new { message = "Compra recebida e estoque atualizado." });
    }

    [HttpGet("sales")]
    public async Task<IActionResult> Sales()
    {
        var rows = await db.SalesOrders.AsNoTracking().Include(x => x.Customer).Include(x => x.Product).OrderByDescending(x => x.CreatedAt).ToListAsync();
        return Ok(rows.Select(x => new { x.Id, customer = x.Customer!.Name, product = x.Product!.Name, x.CustomerId, x.ProductId, x.Quantity, x.UnitPrice, total = x.Quantity * x.UnitPrice, x.Status, x.Notes, x.CreatedAt, x.CompletedAt }));
    }

    [HttpPost("sales")]
    public async Task<IActionResult> CreateSale(OrderInput input)
    {
        if (input.Quantity <= 0 || input.UnitPrice <= 0 || !await db.Customers.AnyAsync(x => x.Id == input.PartnerId) || !await db.Products.AnyAsync(x => x.Id == input.ProductId)) return BadRequest(new { message = "Confira cliente, produto, quantidade e preço." });
        var order = new SalesOrder { CustomerId = input.PartnerId, ProductId = input.ProductId, Quantity = input.Quantity, UnitPrice = input.UnitPrice, Notes = input.Notes ?? "", Status = "Pendente" };
        db.SalesOrders.Add(order); await db.SaveChangesAsync(); return Ok(new { message = "Pedido de venda criado. Confirme para baixar o estoque.", id = order.Id });
    }

    [HttpPost("sales/{id:int}/complete")]
    public async Task<IActionResult> CompleteSale(int id)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var order = await db.SalesOrders.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound(); if (order.Status != "Pendente" || order.Product is null) return Conflict(new { message = "Venda já concluída ou indisponível." });
        if (order.Product.Quantity < order.Quantity) return BadRequest(new { message = $"Estoque insuficiente. Disponível: {order.Product.Quantity}." });
        order.Product.Quantity -= order.Quantity; order.Status = "Concluída"; order.CompletedAt = DateTime.UtcNow;
        db.StockMovements.Add(new StockMovement { ProductId = order.ProductId, Type = "EXIT", Quantity = order.Quantity, Notes = $"Saída referente à venda #{order.Id}" });
        await db.SaveChangesAsync(); await tx.CommitAsync(); return Ok(new { message = "Venda concluída e estoque atualizado." });
    }

    [HttpGet("employees"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Employees() => Ok(await db.AppUsers.AsNoTracking().OrderBy(x => x.FullName).Select(x => new { x.Id, x.FullName, x.Email, x.Phone, x.Role, x.IsActive, x.CreatedAt }).ToListAsync());

    [HttpGet("settings"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Settings() => Ok(await db.SystemSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 1) ?? new SystemSettings());

    [HttpPut("settings"), Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateSettings(SettingsInput input)
    {
        if (string.IsNullOrWhiteSpace(input.CompanyName)) return BadRequest(new { message = "Informe o nome da empresa." });
        var item = await db.SystemSettings.FirstOrDefaultAsync(x => x.Id == 1);
        if (item is null) { item = new SystemSettings { Id = 1 }; db.SystemSettings.Add(item); }
        item.CompanyName = input.CompanyName.Trim(); item.Document = input.Document?.Trim() ?? ""; item.Email = input.Email?.Trim() ?? ""; item.Phone = input.Phone?.Trim() ?? ""; item.Address = input.Address?.Trim() ?? ""; item.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(); return Ok(new { message = "Configurações salvas." });
    }

    [HttpGet("reports")]
    public async Task<IActionResult> Reports()
    {
        var products = await db.Products.AsNoTracking().ToListAsync();
        var sales = await db.SalesOrders.AsNoTracking().Where(x => x.Status == "Concluída").ToListAsync();
        return Ok(new { productCount = products.Count, lowStock = products.Where(x => x.Quantity <= x.MinimumStock).Select(x => new { x.Id, x.Name, x.Quantity, x.MinimumStock }).ToList(), stockValue = products.Sum(x => x.Price * x.Quantity), purchaseCount = await db.PurchaseOrders.CountAsync(), salesCount = await db.SalesOrders.CountAsync(), completedSalesValue = sales.Sum(x => x.Quantity * x.UnitPrice) });
    }
}
