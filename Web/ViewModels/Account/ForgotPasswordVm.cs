using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Account;
public class ForgotPasswordVm
{
    [Required(ErrorMessage = "Ім'я користувача обов'язкове")]
    public string Username { get; set; }
}