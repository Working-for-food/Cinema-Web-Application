using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Web.Helpers;

public class StrongPasswordAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var password = value as string;

        if (string.IsNullOrEmpty(password))
        {
            return ValidationResult.Success;
        }

        var errors = new List<string>();

        if (password.Length < 8)
            errors.Add("8 символів");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            errors.Add("1 велику літеру");

        if (!Regex.IsMatch(password, @"[a-z]"))
            errors.Add("1 малу літеру");

        if (!Regex.IsMatch(password, @"[0-9]"))
            errors.Add("1 цифру");

        if (!Regex.IsMatch(password, @"[\W_]")) 
            errors.Add("1 спецсимвол");

        if (errors.Count > 0)
        {
            return new ValidationResult($"Пароль повинен містити як мінімум: {string.Join(", ", errors)}.");
        }

        return ValidationResult.Success;
    }
}