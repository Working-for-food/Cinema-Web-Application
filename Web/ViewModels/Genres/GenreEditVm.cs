using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Genres;

public class GenreEditVm
{
    public int Id { get; set; }

    [Required]
    [StringLength(60, MinimumLength = 2)]
    public string Name { get; set; } = "";
}
