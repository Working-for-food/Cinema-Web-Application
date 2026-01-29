using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.Admin;

public class MovieIndexVm
{
    public IEnumerable<Application.DTOs.MovieDto> Movies { get; set; } = new List<Application.DTOs.MovieDto>();
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
}

public class MovieEditVm : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Movie title is required")]
    public string Title { get; set; } = "";

    public string? Description { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? ReleaseDate { get; set; }

    
    [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 min")]
    public int? Duration { get; set; }

    
    [RegularExpression(@"^$|^[A-Za-z]{2}$", ErrorMessage = "Production country code must be 2 letters or empty.")]
    public string? ProductionCountryCode { get; set; }

    public int? DirectorId { get; set; }

    [Display(Name = "Genres")]
    public List<int> SelectedGenreIds { get; set; } = new();

    public MultiSelectList? GenreList { get; set; }
    public SelectList? CountryList { get; set; }
    public SelectList? DirectorList { get; set; }

    [Display(Name = "Actors")]
    public List<int> SelectedActorIds { get; set; } = new();
    public MultiSelectList? ActorList { get; set; }

    [Display(Name = "Countries")]
    public List<string> SelectedCountryCodes { get; set; } = new();
    public MultiSelectList? CountryMultiList { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SelectedGenreIds == null || SelectedGenreIds.Count == 0)
            yield return new ValidationResult("Select at least one genre.", new[] { nameof(SelectedGenreIds) });

        
        foreach (var c in SelectedCountryCodes ?? new List<string>())
        {
            if (string.IsNullOrWhiteSpace(c)) continue;
            var x = c.Trim();
            if (x.Length != 2 || !x.All(char.IsLetter))
                yield return new ValidationResult("Country code in list must be 2 letters.", new[] { nameof(SelectedCountryCodes) });
        }
    }
}
