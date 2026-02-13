using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Account;

public class LoginVm
{
    [Required(ErrorMessage = "Ім'я користувача обов'язкове")]
    [Display(Name = "Ім'я користувача")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Пароль обов'язковий")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; }

    [Display(Name = "Запам'ятати мене")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}