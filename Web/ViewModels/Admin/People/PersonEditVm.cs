using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.Admin.People;

public class PersonEditVm
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string FirstName { get; set; } = "";

    [StringLength(60)]
    public string? MiddleName { get; set; }

    [Required, StringLength(60)]
    public string LastName { get; set; } = "";

    [DataType(DataType.Date)]
    public DateOnly? BirthDate { get; set; }

    [RegularExpression(@"^$|^[A-Za-z]{2}$", ErrorMessage = "Country code must be 2 letters (e.g. UA) or empty.")]
    public string? CountryCode { get; set; }

    [StringLength(700)]
    public string? PhotoUrl { get; set; }

    public SelectList? CountryList { get; set; }
}
