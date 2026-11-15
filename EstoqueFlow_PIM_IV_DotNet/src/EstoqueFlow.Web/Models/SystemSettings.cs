using System.ComponentModel.DataAnnotations;

namespace EstoqueFlow.Web.Models;

public class SystemSettings
{
    // O sistema mantém um único registro de configurações (Id = 1).
    public int Id { get; set; } = 1;

    [Required, StringLength(140)]
    public string CompanyName { get; set; } = "Minha empresa";

    [StringLength(20)]
    public string Document { get; set; } = "";

    [StringLength(120), EmailAddress]
    public string Email { get; set; } = "";

    [StringLength(30)]
    public string Phone { get; set; } = "";

    [StringLength(180)]
    public string Address { get; set; } = "";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
