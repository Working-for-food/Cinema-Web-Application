using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Web.Helpers;

namespace Web.ViewModels.Account;

public class RegisterVm
{
    [Required(ErrorMessage = "Вкажіть електронну пошту")]
    [EmailAddress(ErrorMessage = "Невірний формат електронної пошти")]
    [Remote(action: "VerifyEmail", controller: "Account")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Вкажіть ім'я користувача")]
    [Remote(action: "VerifyUsername", controller: "Account")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Вкажіть дату народження")]
    [MinimumAge(12, ErrorMessage = "Реєстрація дозволена лише особам від 12 років.")]
    [DataType(DataType.Date)]
    public DateOnly? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Введіть пароль")]
    [DataType(DataType.Password)]
    [StrongPassword]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Підтвердіть пароль")]
    [Compare("Password", ErrorMessage = "Паролі не співпадають.")]
    public string ConfirmPassword { get; set; } = null!;
}