using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.Admin.Statistics;

public class BookingStatisticsIndexVm
{
    // Filters
    public DateTime? SessionFrom { get; set; }
    public DateTime? SessionTo { get; set; }

    public DateTime? BookingFrom { get; set; }
    public DateTime? BookingTo { get; set; }

    public int CinemaId { get; set; }
    public int HallId { get; set; }

    public string MovieTitle { get; set; } = "";
    public int MovieId { get; set; }

    public int? SessionId { get; set; }

    public List<PresentationType> PresentationTypes { get; set; } = new();
    public List<SeatCategory> SeatCategories { get; set; } = new();

    public bool IncludeCancelledSessions { get; set; }
    public bool IncludeFinishedSessions { get; set; } = true;
    public bool IncludeDeletedBookingsInPeriod { get; set; }

    // Lookups
    public List<SelectListItem> Cinemas { get; set; } = new();
    public List<SelectListItem> Halls { get; set; } = new();
    public List<SelectListItem> Movies { get; set; } = new();

    // Result
    public BookingStatisticsResult? Result { get; set; }
}
