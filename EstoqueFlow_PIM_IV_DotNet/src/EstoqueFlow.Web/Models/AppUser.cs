using System.ComponentModel.DataAnnotations;

namespace EstoqueFlow.Web.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string FullName { get; set; } = "";

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = "";

    [Required, StringLength(160)]
    public string NormalizedEmail { get; set; } = "";

    [StringLength(30)]
    public string Phone { get; set; } = "";

    [Required, StringLength(30)]
    public string Role { get; set; } = "Funcionario";

    [Required, StringLength(500)]
    public string PasswordHash { get; set; } = "";

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
