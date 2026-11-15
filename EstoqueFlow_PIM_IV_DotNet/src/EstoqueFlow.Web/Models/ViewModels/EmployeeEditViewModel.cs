using System.ComponentModel.DataAnnotations;

namespace EstoqueFlow.Web.Models.ViewModels;

public class EmployeeEditViewModel
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    [Display(Name = "Nome completo")]
    public string FullName { get; set; } = "";

    [Required, EmailAddress, StringLength(160)]
    [Display(Name = "E-mail de acesso")]
    public string Email { get; set; } = "";

    [StringLength(30)]
    [Display(Name = "Telefone")]
    public string Phone { get; set; } = "";

    [Required]
    [Display(Name = "Perfil de acesso")]
    public string Role { get; set; } = "Funcionario";

    [Display(Name = "Usuário ativo")]
    public bool IsActive { get; set; }
}
