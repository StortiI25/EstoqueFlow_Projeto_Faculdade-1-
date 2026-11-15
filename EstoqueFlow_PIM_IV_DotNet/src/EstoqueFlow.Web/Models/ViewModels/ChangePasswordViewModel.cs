using System.ComponentModel.DataAnnotations;

namespace EstoqueFlow.Web.Models.ViewModels;

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password)]
    [Display(Name = "Senha atual")]
    public string CurrentPassword { get; set; } = "";

    [Required, MinLength(10), DataType(DataType.Password)]
    [Display(Name = "Nova senha (mínimo de 10 caracteres)")]
    public string NewPassword { get; set; } = "";

    [Required, Compare(nameof(NewPassword)), DataType(DataType.Password)]
    [Display(Name = "Confirme a nova senha")]
    public string ConfirmNewPassword { get; set; } = "";
}
