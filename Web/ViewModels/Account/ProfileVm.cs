using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Account;

public class ProfileVm
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; }

    [Display(Name = "Phone Number")]
    [Phone]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateOnly? DateOfBirth { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();

    [Display(Name = "Two-Factor Authentication")]
    public bool TwoFactorEnabled { get; set; }
    public string? StatusMessage { get; set; }
}