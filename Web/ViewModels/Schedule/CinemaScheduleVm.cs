using Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.Schedule;

public class CinemaScheduleVm
{
    public int CinemaId { get; set; }
    public string CinemaName { get; set; } = "";
    public DateOnly SelectedDate { get; set; }
    public List<SelectListItem> Cinemas { get; set; } = new();
    public List<ScheduleDayVm> Days { get; set; } = new();
    public List<MovieScheduleCardVm> Movies { get; set; } = new();

    public class ScheduleDayVm
    {
        public DateOnly Date { get; set; }
        public string LabelTop { get; set; } = "";     // "10 лютого"
        public string LabelBottom { get; set; } = "";  // "завтра/середа..."
        public bool IsActive { get; set; }
    }

    public class MovieScheduleCardVm
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = "";
        public string? PosterUrl { get; set; }
        public string? AgeLabel { get; set; }
        public List<SessionTimeVm> Times { get; set; } = new();
    }

    public class SessionTimeVm
    {
        public int SessionId { get; set; }
        public TimeOnly Time { get; set; }
        public PresentationType PresentationType { get; set; }
        public bool IsSoldOut { get; set; }
    }
}