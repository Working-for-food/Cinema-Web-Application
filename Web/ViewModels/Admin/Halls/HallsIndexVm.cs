using Application.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.Admin.Halls;

public class HallsIndexVm
{
    public int? CinemaId { get; set; }
    public List<SelectListItem> Cinemas { get; set; } = new();
    public List<HallListDto> Halls { get; set; } = new();
}
