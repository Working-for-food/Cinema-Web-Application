using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.Cinemas;

public class UserCinemasIndexVm
{
    public string? SelectedCity { get; set; }
    public string? SearchQuery { get; set; }
    public List<SelectListItem> Cities { get; set; } = new();
    public IEnumerable<Application.DTOs.CinemaDto> Cinemas { get; set; } = new List<Application.DTOs.CinemaDto>();
}