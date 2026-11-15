using System.Text.Json.Serialization;

namespace EstoqueFlow.Mobile.Models;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int MinimumStock { get; set; }

    [JsonIgnore]
    public string StockText => $"Quantidade: {Quantity} | Mínimo: {MinimumStock}";
}
