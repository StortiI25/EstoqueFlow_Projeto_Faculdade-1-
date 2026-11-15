using System.ComponentModel.DataAnnotations;

namespace EstoqueFlow.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Password { get; set; } = "";

    [Display(Name = "Manter conectado neste computador")]
    public bool RememberMe { get; set; }
}
