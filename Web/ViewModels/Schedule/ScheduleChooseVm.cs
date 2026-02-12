using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.Schedule;

public sealed class ScheduleChooseVm
{
    public string? City { get; set; }
    public int? CinemaId { get; set; }

    public List<SelectListItem> Cities { get; set; } = new();
    public List<CinemaCardVm> Cinemas { get; set; } = new();

    public bool HasCity => !string.IsNullOrWhiteSpace(City);
    public sealed class CinemaCardVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string City { get; set; } = "";
        public string? Address { get; set; }
        public string? ImageUrl { get; set; }
    }

}

