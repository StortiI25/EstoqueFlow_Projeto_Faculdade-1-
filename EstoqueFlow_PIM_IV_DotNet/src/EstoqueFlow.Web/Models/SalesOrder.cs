using System.ComponentModel.DataAnnotations;

namespace EstoqueFlow.Web.Models;

public class SalesOrder
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    [Range(1, 1000000)] public int Quantity { get; set; }
    [Range(typeof(decimal), "0,01", "9999999")] public decimal UnitPrice { get; set; }
    [StringLength(250)] public string Notes { get; set; } = "";
    [StringLength(20)] public string Status { get; set; } = "Pendente";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public decimal Total => Quantity * UnitPrice;
}
