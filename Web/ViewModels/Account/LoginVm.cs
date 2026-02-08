using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Account;

public class LoginVm
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}