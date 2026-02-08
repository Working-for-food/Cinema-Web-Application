using System.ComponentModel.DataAnnotations;

namespace Web.Helpers;

public class MinimumAgeAttribute : ValidationAttribute
{
    private readonly int _minimumAge;

    public MinimumAgeAttribute(int minimumAge)
    {
        _minimumAge = minimumAge;
        ErrorMessage = "Вам має бути не менше {0} років.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateOnly date)
        {
            var age = DateTime.Today.Year - date.Year;

            if (date > DateOnly.FromDateTime(DateTime.Today.AddYears(-age)))
            {
                age--;
            }

            if (age < _minimumAge)
            {
                return new ValidationResult(string.Format(ErrorMessageString, _minimumAge));
            }
        }
        return ValidationResult.Success;
    }
}