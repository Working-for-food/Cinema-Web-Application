using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Account;

public class ChangePasswordVm
{
    [Required(ErrorMessage = "Введіть поточний пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Поточний пароль")]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "Введіть новий пароль")]
    [StringLength(100, ErrorMessage = "{0} має бути мінімум {2} символів.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Новий пароль")]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Підтвердження нового пароля")]
    [Compare("NewPassword", ErrorMessage = "Паролі не співпадають.")]
    public string ConfirmNewPassword { get; set; }
}