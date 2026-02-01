using Application.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.Admin.Sessions;

public class SessionsIndexVm
{
    public int? CinemaId { get; set; }
    public int? HallId { get; set; }
    public int? MovieId { get; set; }
    public string? MovieTitle { get; set; }

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public bool IncludeCancelled { get; set; }
    public bool IncludeFinished { get; set; }
    public string? Sort { get; set; }

    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }

    public List<SelectListItem> Cinemas { get; set; } = new();
    public List<SelectListItem> Halls { get; set; } = new();
    public List<SelectListItem> Movies { get; set; } = new();
    public List<SelectListItem> SortOptions { get; set; } = new();

    public List<SessionListDto> Sessions { get; set; } = new();
}
