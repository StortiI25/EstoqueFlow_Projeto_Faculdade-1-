using System.ComponentModel.DataAnnotations;

namespace EstoqueFlow.Web.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [StringLength(60)]
    public string Category { get; set; } = "";

    [Range(0, 9999999)]
    public decimal Price { get; set; }

    [Range(0, 1000000)]
    public int Quantity { get; set; }

    [Range(0, 1000000)]
    public int MinimumStock { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsLowStock => Quantity <= MinimumStock;
}
