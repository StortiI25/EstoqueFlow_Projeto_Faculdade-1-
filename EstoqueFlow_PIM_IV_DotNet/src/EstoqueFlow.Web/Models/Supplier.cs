using System.ComponentModel.DataAnnotations;

namespace EstoqueFlow.Web.Models;

public class Supplier
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Name { get; set; } = "";
    [StringLength(20)] public string Document { get; set; } = "";
    [StringLength(120), EmailAddress] public string Email { get; set; } = "";
    [StringLength(30)] public string Phone { get; set; } = "";
    [StringLength(180)] public string Address { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
