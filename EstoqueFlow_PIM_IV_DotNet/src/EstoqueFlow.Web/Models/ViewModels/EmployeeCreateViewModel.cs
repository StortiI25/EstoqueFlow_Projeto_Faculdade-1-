using System.ComponentModel.DataAnnotations;

namespace EstoqueFlow.Web.Models.ViewModels;

public class EmployeeCreateViewModel
{
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

    [Required, MinLength(10), DataType(DataType.Password)]
    [Display(Name = "Senha inicial (mínimo de 10 caracteres)")]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password)), DataType(DataType.Password)]
    [Display(Name = "Confirme a senha")]
    public string ConfirmPassword { get; set; } = "";
}
