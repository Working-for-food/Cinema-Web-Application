using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.Admin
{
    public class MovieIndexVm
    {
        public IEnumerable<Application.DTOs.MovieDto> Movies { get; set; } = new List<Application.DTOs.MovieDto>();
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
    }

    public class MovieEditVm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Movie title is required")]
        public string Title { get; set; }

        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? ReleaseDate { get; set; }

        [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 min")]
        public int? Duration { get; set; }

        public string? ProductionCountryCode { get; set; }

        [Display(Name = "Genres")]
        [MinLength(1, ErrorMessage = "Select at least one genre")]
        public List<int> SelectedGenreIds { get; set; } = new();

        public MultiSelectList? GenreList { get; set; }
    }
}
