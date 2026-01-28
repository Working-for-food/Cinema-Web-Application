using Application.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.Admin.Cinemas;

public class CinemasIndexVm
{
    public string? City { get; set; }
    public string? Search { get; set; }
    public string? Sort { get; set; }

    public List<SelectListItem> Cities { get; set; } = new();
    public List<SelectListItem> SortOptions { get; set; } = new();

    public List<CinemaListDto> Cinemas { get; set; } = new();
}
