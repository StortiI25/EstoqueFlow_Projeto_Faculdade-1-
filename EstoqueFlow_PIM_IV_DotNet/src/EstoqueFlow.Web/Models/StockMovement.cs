using System.ComponentModel.DataAnnotations;

namespace EstoqueFlow.Web.Models;

public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Required]
    public string Type { get; set; } = ""; // ENTRY ou EXIT

    [Range(1, 1000000)]
    public int Quantity { get; set; }

    [StringLength(250)]
    public string Notes { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
