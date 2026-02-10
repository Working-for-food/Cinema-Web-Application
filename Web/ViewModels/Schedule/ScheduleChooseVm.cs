using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.Schedule;

public sealed class ScheduleChooseVm
{
    public string? City { get; set; }
    public int? CinemaId { get; set; }

    public List<SelectListItem> Cities { get; set; } = new();
    public List<SelectListItem> Cinemas { get; set; } = new();

    public bool HasCity => !string.IsNullOrWhiteSpace(City);
}

