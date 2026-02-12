using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public enum StatisticsDateMode
{
    SessionStart = 0,
    BookingBookedAt = 1
}

public sealed record BookingStatisticsFilter
{
    // --- Session scope ---
    public DateTime? SessionFrom { get; init; }
    public DateTime? SessionTo { get; init; }

    public int? CinemaId { get; init; }
    public int? HallId { get; init; }
    public int? MovieId { get; init; }
    public int? SessionId { get; init; }

    public IReadOnlyList<PresentationType> PresentationTypes { get; init; } = Array.Empty<PresentationType>();
    public bool IncludeCancelledSessions { get; init; } = false;
    public bool IncludeFinishedSessions { get; init; } = true;

    // --- Seat scope (впливає на "місця" та "дохід по місцях") ---
    public IReadOnlyList<SeatCategory> SeatCategories { get; init; } = Array.Empty<SeatCategory>();

    // --- Booking period ("продажі за період") ---
    public DateTime? BookingFrom { get; init; }
    public DateTime? BookingTo { get; init; }

    
    public bool IncludeDeletedBookingsInPeriod { get; init; } = false;

    public StatisticsDateMode DayGroupingMode { get; init; } = StatisticsDateMode.SessionStart;
}

public sealed record BookingStatisticsSummary
{
    // Sessions
    public int SessionsCount { get; init; }
    public int CancelledSessionsCount { get; init; }
    public int FinishedSessionsCount { get; init; }
    public int ActiveSessionsCount { get; init; }

    // Seats (стан "на зараз")
    public int TotalSeats { get; init; }
    public int BookedSeatsNow { get; init; }
    public int FreeSeatsNow { get; init; }
    public decimal OccupancyNowPercent { get; init; }

    public decimal PotentialRevenue { get; init; }
    public decimal RevenueNow { get; init; }
    public decimal RemainingPotentialRevenue { get; init; }

    public int BookingsCountNow { get; init; }
    public decimal AverageTicketPriceNow { get; init; }
    public decimal AverageBookingAmountNow { get; init; }

    // Sales за період (по Booking.BookedAt)
    public int BookingsCountPeriod { get; init; }
    public int SeatsSoldPeriod { get; init; }
    public decimal RevenuePeriod { get; init; }
    public decimal AverageTicketPricePeriod { get; init; }
    public decimal AverageBookingAmountPeriod { get; init; }

    public int CancelledBookingsCountPeriod { get; init; }
    public decimal CancelledRevenuePeriod { get; init; }
}

public sealed record StatsByDayRow
{
    public DateOnly Day { get; init; }

    public int SessionsCount { get; init; }
    public int TotalSeats { get; init; }

    public int BookedSeatsNow { get; init; }
    public decimal RevenueNow { get; init; }
    public decimal OccupancyNowPercent { get; init; }

    public int SeatsSoldPeriod { get; init; }
    public int BookingsCountPeriod { get; init; }
    public decimal RevenuePeriod { get; init; }
}

public sealed record StatsByCinemaRow
{
    public int CinemaId { get; init; }
    public string CinemaName { get; init; } = "";

    public int SessionsCount { get; init; }
    public int TotalSeats { get; init; }

    public int BookedSeatsNow { get; init; }
    public decimal RevenueNow { get; init; }
    public decimal PotentialRevenue { get; init; }
    public decimal OccupancyNowPercent { get; init; }

    public int BookingsCountNow { get; init; }

    public int SeatsSoldPeriod { get; init; }
    public int BookingsCountPeriod { get; init; }
    public decimal RevenuePeriod { get; init; }
}

public sealed record StatsByHallRow
{
    public int HallId { get; init; }
    public string HallName { get; init; } = "";
    public int CinemaId { get; init; }
    public string CinemaName { get; init; } = "";

    public int SessionsCount { get; init; }
    public int TotalSeats { get; init; }
    public int BookedSeatsNow { get; init; }
    public decimal RevenueNow { get; init; }
    public decimal PotentialRevenue { get; init; }
    public decimal OccupancyNowPercent { get; init; }
    public int BookingsCountNow { get; init; }

    public int SeatsSoldPeriod { get; init; }
    public int BookingsCountPeriod { get; init; }
    public decimal RevenuePeriod { get; init; }
}

public sealed record StatsByMovieRow
{
    public int MovieId { get; init; }
    public string MovieTitle { get; init; } = "";

    public int SessionsCount { get; init; }
    public int TotalSeats { get; init; }
    public int BookedSeatsNow { get; init; }
    public decimal RevenueNow { get; init; }
    public decimal PotentialRevenue { get; init; }
    public decimal OccupancyNowPercent { get; init; }
    public int BookingsCountNow { get; init; }

    public int SeatsSoldPeriod { get; init; }
    public int BookingsCountPeriod { get; init; }
    public decimal RevenuePeriod { get; init; }
}

public sealed record StatsBySessionRow
{
    public int SessionId { get; init; }
    public DateTime StartTime { get; init; }
    public string MovieTitle { get; init; } = "";
    public string CinemaName { get; init; } = "";
    public string HallName { get; init; } = "";
    public PresentationType PresentationType { get; init; }

    public int TotalSeats { get; init; }
    public int BookedSeatsNow { get; init; }
    public decimal RevenueNow { get; init; }
    public decimal PotentialRevenue { get; init; }
    public decimal OccupancyNowPercent { get; init; }
    public int BookingsCountNow { get; init; }

    public int SeatsSoldPeriod { get; init; }
    public int BookingsCountPeriod { get; init; }
    public decimal RevenuePeriod { get; init; }
}

public sealed record StatsByPresentationTypeRow
{
    public PresentationType PresentationType { get; init; }

    public int SessionsCount { get; init; }
    public int TotalSeats { get; init; }
    public int BookedSeatsNow { get; init; }
    public decimal RevenueNow { get; init; }
    public decimal PotentialRevenue { get; init; }
    public decimal OccupancyNowPercent { get; init; }
    public int BookingsCountNow { get; init; }

    public int SeatsSoldPeriod { get; init; }
    public int BookingsCountPeriod { get; init; }
    public decimal RevenuePeriod { get; init; }
}

public sealed record StatsBySeatCategoryRow
{
    public SeatCategory SeatCategory { get; init; }

    public int SessionsCount { get; init; }
    public int TotalSeats { get; init; }
    public int BookedSeatsNow { get; init; }
    public decimal RevenueNow { get; init; }
    public decimal PotentialRevenue { get; init; }
    public decimal OccupancyNowPercent { get; init; }
    public int BookingsCountNow { get; init; }

    public int SeatsSoldPeriod { get; init; }
    public int BookingsCountPeriod { get; init; }
    public decimal RevenuePeriod { get; init; }
}

public sealed record BookingStatisticsResult
{
    public BookingStatisticsSummary Summary { get; init; } = new();

    public IReadOnlyList<StatsByCinemaRow> ByCinemas { get; init; } = Array.Empty<StatsByCinemaRow>();
    public IReadOnlyList<StatsByHallRow> ByHalls { get; init; } = Array.Empty<StatsByHallRow>();
    public IReadOnlyList<StatsByMovieRow> ByMovies { get; init; } = Array.Empty<StatsByMovieRow>();
    public IReadOnlyList<StatsBySessionRow> BySessions { get; init; } = Array.Empty<StatsBySessionRow>();
    public IReadOnlyList<StatsByPresentationTypeRow> ByPresentationTypes { get; init; } = Array.Empty<StatsByPresentationTypeRow>();
    public IReadOnlyList<StatsBySeatCategoryRow> BySeatCategories { get; init; } = Array.Empty<StatsBySeatCategoryRow>();

    public IReadOnlyList<StatsByDayRow> BySessionStartDay { get; init; } = Array.Empty<StatsByDayRow>();
    public IReadOnlyList<StatsByDayRow> ByBookingDay { get; init; } = Array.Empty<StatsByDayRow>();
}
