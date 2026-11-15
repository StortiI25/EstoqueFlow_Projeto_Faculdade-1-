using EstoqueFlow.Web.Data;
using EstoqueFlow.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Services;

public class StockService(EstoqueFlowContext db)
{
    public async Task<(bool Success, string Message)> RegisterAsync(
        int productId, int quantity, string type, string notes = "")
    {
        if (quantity <= 0)
            return (false, "A quantidade deve ser maior que zero.");

        type = type.Trim().ToUpperInvariant();
        if (type is not ("ENTRY" or "EXIT"))
            return (false, "Tipo deve ser ENTRY ou EXIT.");

        await using var transaction = await db.Database.BeginTransactionAsync();

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product is null)
            return (false, "Produto não encontrado.");

        if (type == "EXIT" && product.Quantity < quantity)
            return (false, "Estoque insuficiente para essa saída.");

        product.Quantity += type == "ENTRY" ? quantity : -quantity;

        db.StockMovements.Add(new StockMovement
        {
            ProductId = productId,
            Quantity = quantity,
            Type = type,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true, "Movimentação registrada com sucesso.");
    }
}
