using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Account;
public class ResetPasswordVm
{
    [Required]
    public string UserId { get; set; }

    [Required]
    public string Code { get; set; }

    [Required(ErrorMessage = "Пароль обов'язковий")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Підтвердження пароля")]
    public string ConfirmPassword { get; set; }
}