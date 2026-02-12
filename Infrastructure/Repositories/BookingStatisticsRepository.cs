using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BookingStatisticsRepository : IBookingStatisticsRepository
{
    private readonly CinemaDbContext _db;

    public BookingStatisticsRepository(CinemaDbContext db)
    {
        _db = db;
    }

    public async Task<BookingStatisticsResult> GetBookingStatisticsAsync(BookingStatisticsFilter filter, CancellationToken ct)
    {
        var now = DateTime.Now;

        var bf = filter.BookingFrom;
        var bt = filter.BookingTo;

        IQueryable<Session> sessionsQ = _db.Sessions.AsNoTracking();

        // --- Session-scope filters ---
        if (filter.SessionId is > 0)
            sessionsQ = sessionsQ.Where(s => s.Id == filter.SessionId.Value);

        if (filter.CinemaId is > 0)
            sessionsQ = sessionsQ.Where(s => s.Hall.CinemaId == filter.CinemaId.Value);

        if (filter.HallId is > 0)
            sessionsQ = sessionsQ.Where(s => s.HallId == filter.HallId.Value);

        if (filter.MovieId is > 0)
            sessionsQ = sessionsQ.Where(s => s.MovieId == filter.MovieId.Value);

        if (filter.PresentationTypes.Count > 0)
            sessionsQ = sessionsQ.Where(s => filter.PresentationTypes.Contains(s.PresentationType));

        if (!filter.IncludeCancelledSessions)
            sessionsQ = sessionsQ.Where(s => !s.IsCancelled);

        if (!filter.IncludeFinishedSessions)
            sessionsQ = sessionsQ.Where(s => s.EndTime > now);

        if (filter.SessionFrom.HasValue)
            sessionsQ = sessionsQ.Where(s => s.StartTime >= filter.SessionFrom.Value);

        if (filter.SessionTo.HasValue)
            sessionsQ = sessionsQ.Where(s => s.StartTime <= filter.SessionTo.Value);

        var sessionIdsQ = sessionsQ.Select(s => s.Id);

        // Seats within session scope
        IQueryable<SessionSeat> seatsQ = _db.SessionSeats
            .AsNoTracking()
            .Where(ss => sessionIdsQ.Contains(ss.SessionId));

        if (filter.SeatCategories.Count > 0)
            seatsQ = seatsQ.Where(ss => filter.SeatCategories.Contains(ss.Seat.Category));

        // --- "Now" metrics (стан місць зараз) ---
        var totalSeats = await seatsQ.CountAsync(ct);
        var bookedSeatsNow = await seatsQ.CountAsync(ss => ss.BookingId != null, ct);
        var freeSeatsNow = totalSeats - bookedSeatsNow;

        var potentialRevenue = await seatsQ.SumAsync(ss => (decimal?)ss.Price, ct) ?? 0m;
        var revenueNow = await seatsQ
            .Where(ss => ss.BookingId != null)
            .SumAsync(ss => (decimal?)ss.Price, ct) ?? 0m;

        var remainingPotentialRevenue = potentialRevenue - revenueNow;

        var bookingsNowCount = await seatsQ
            .Where(ss => ss.BookingId != null)
            .Select(ss => ss.BookingId!.Value)
            .Distinct()
            .CountAsync(ct);

        var occupancyNow = totalSeats == 0 ? 0m : Math.Round(bookedSeatsNow * 100m / totalSeats, 2);
        var avgTicketNow = bookedSeatsNow == 0 ? 0m : Math.Round(revenueNow / bookedSeatsNow, 2);
        var avgBookingNow = bookingsNowCount == 0 ? 0m : Math.Round(revenueNow / bookingsNowCount, 2);

        // --- Period sales (по Booking.BookedAt) ---
        IQueryable<SessionSeat> soldSeatsQ = seatsQ.Where(ss => ss.BookingId != null);

        if (bf.HasValue)
            soldSeatsQ = soldSeatsQ.Where(ss => ss.Booking!.BookedAt >= bf.Value);
        if (bt.HasValue)
            soldSeatsQ = soldSeatsQ.Where(ss => ss.Booking!.BookedAt <= bt.Value);

        var seatsSoldPeriod = await soldSeatsQ.CountAsync(ct);
        var revenuePeriodSeats = await soldSeatsQ.SumAsync(ss => (decimal?)ss.Price, ct) ?? 0m;
        var bookingsCountPeriodFromSeats = await soldSeatsQ
            .Select(ss => ss.BookingId!.Value)
            .Distinct()
            .CountAsync(ct);

        // Bookings table for cancellations in period
        IQueryable<Booking> bookingsPeriodQ = _db.Bookings.AsNoTracking().Where(b => sessionIdsQ.Contains(b.SessionId));
        if (bf.HasValue)
            bookingsPeriodQ = bookingsPeriodQ.Where(b => b.BookedAt >= bf.Value);
        if (bt.HasValue)
            bookingsPeriodQ = bookingsPeriodQ.Where(b => b.BookedAt <= bt.Value);

        var cancelledBookingsQ = bookingsPeriodQ.Where(b => b.IsDeleted);
        var cancelledBookingsCountPeriod = await cancelledBookingsQ.CountAsync(ct);
        var cancelledRevenuePeriod = await cancelledBookingsQ.SumAsync(b => (decimal?)b.TotalAmount, ct) ?? 0m;

        var revenuePeriod = filter.IncludeDeletedBookingsInPeriod
            ? revenuePeriodSeats + cancelledRevenuePeriod
            : revenuePeriodSeats;

        var avgTicketPeriod = seatsSoldPeriod == 0 ? 0m : Math.Round(revenuePeriodSeats / seatsSoldPeriod, 2);
        var avgBookingPeriod = bookingsCountPeriodFromSeats == 0 ? 0m : Math.Round(revenuePeriodSeats / bookingsCountPeriodFromSeats, 2);

        // Sessions counts (в межах відфільтрованого sessionsQ)
        var sessionsCount = await sessionsQ.CountAsync(ct);
        var cancelledSessionsCount = await sessionsQ.CountAsync(s => s.IsCancelled, ct);
        var finishedSessionsCount = await sessionsQ.CountAsync(s => !s.IsCancelled && s.EndTime <= now, ct);
        var activeSessionsCount = sessionsCount - cancelledSessionsCount - finishedSessionsCount;
        if (activeSessionsCount < 0) activeSessionsCount = 0;

        var summary = new BookingStatisticsSummary
        {
            SessionsCount = sessionsCount,
            CancelledSessionsCount = cancelledSessionsCount,
            FinishedSessionsCount = finishedSessionsCount,
            ActiveSessionsCount = activeSessionsCount,

            TotalSeats = totalSeats,
            BookedSeatsNow = bookedSeatsNow,
            FreeSeatsNow = freeSeatsNow,
            OccupancyNowPercent = occupancyNow,

            PotentialRevenue = potentialRevenue,
            RevenueNow = revenueNow,
            RemainingPotentialRevenue = remainingPotentialRevenue,

            BookingsCountNow = bookingsNowCount,
            AverageTicketPriceNow = avgTicketNow,
            AverageBookingAmountNow = avgBookingNow,

            BookingsCountPeriod = bookingsCountPeriodFromSeats,
            SeatsSoldPeriod = seatsSoldPeriod,
            RevenuePeriod = revenuePeriod,
            AverageTicketPricePeriod = avgTicketPeriod,
            AverageBookingAmountPeriod = avgBookingPeriod,

            CancelledBookingsCountPeriod = cancelledBookingsCountPeriod,
            CancelledRevenuePeriod = cancelledRevenuePeriod
        };

        // --- By Cinemas ---
        var byCinemas = await seatsQ
            .GroupBy(ss => new { ss.Session.Hall.CinemaId, CinemaName = ss.Session.Hall.Cinema.Name })
            .Select(g => new StatsByCinemaRow
            {
                CinemaId = g.Key.CinemaId,
                CinemaName = g.Key.CinemaName,

                SessionsCount = g.Select(x => x.SessionId).Distinct().Count(),
                TotalSeats = g.Count(),

                BookedSeatsNow = g.Count(x => x.BookingId != null),
                RevenueNow = g.Where(x => x.BookingId != null).Sum(x => (decimal?)x.Price) ?? 0m,
                PotentialRevenue = g.Sum(x => (decimal?)x.Price) ?? 0m,

                OccupancyNowPercent = g.Count() == 0
                    ? 0m
                    : Math.Round(g.Count(x => x.BookingId != null) * 100m / g.Count(), 2),

                BookingsCountNow = g.Where(x => x.BookingId != null).Select(x => x.BookingId!.Value).Distinct().Count(),

                SeatsSoldPeriod = g.Count(x =>
                    x.BookingId != null
                    && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                    && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value)),

                BookingsCountPeriod = g.Where(x =>
                        x.BookingId != null
                        && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Select(x => x.BookingId!.Value)
                    .Distinct()
                    .Count(),

                RevenuePeriod = g.Where(x =>
                        x.BookingId != null
                        && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Sum(x => (decimal?)x.Price) ?? 0m
            })
            .OrderByDescending(x => x.RevenueNow)
            .ToListAsync(ct);

        // --- By Halls ---
        var byHalls = await seatsQ
            .GroupBy(ss => new
            {
                ss.Session.HallId,
                HallName = ss.Session.Hall.Name,
                CinemaId = ss.Session.Hall.CinemaId,
                CinemaName = ss.Session.Hall.Cinema.Name
            })
            .Select(g => new StatsByHallRow
            {
                HallId = g.Key.HallId,
                HallName = g.Key.HallName,
                CinemaId = g.Key.CinemaId,
                CinemaName = g.Key.CinemaName,

                SessionsCount = g.Select(x => x.SessionId).Distinct().Count(),
                TotalSeats = g.Count(),

                BookedSeatsNow = g.Count(x => x.BookingId != null),
                RevenueNow = g.Where(x => x.BookingId != null).Sum(x => (decimal?)x.Price) ?? 0m,
                PotentialRevenue = g.Sum(x => (decimal?)x.Price) ?? 0m,

                OccupancyNowPercent = g.Count() == 0
                    ? 0m
                    : Math.Round(g.Count(x => x.BookingId != null) * 100m / g.Count(), 2),

                BookingsCountNow = g.Where(x => x.BookingId != null).Select(x => x.BookingId!.Value).Distinct().Count(),

                SeatsSoldPeriod = g.Count(x =>
                    x.BookingId != null
                    && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                    && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value)),

                BookingsCountPeriod = g.Where(x =>
                        x.BookingId != null
                        && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Select(x => x.BookingId!.Value)
                    .Distinct()
                    .Count(),

                RevenuePeriod = g.Where(x =>
                        x.BookingId != null
                        && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Sum(x => (decimal?)x.Price) ?? 0m
            })
            .OrderByDescending(x => x.RevenueNow)
            .ToListAsync(ct);

        // --- By Movies (top 50) ---
        var byMovies = await seatsQ
            .GroupBy(ss => new { ss.Session.MovieId, MovieTitle = ss.Session.Movie.Title })
            .Select(g => new StatsByMovieRow
            {
                MovieId = g.Key.MovieId,
                MovieTitle = g.Key.MovieTitle,

                SessionsCount = g.Select(x => x.SessionId).Distinct().Count(),
                TotalSeats = g.Count(),

                BookedSeatsNow = g.Count(x => x.BookingId != null),
                RevenueNow = g.Where(x => x.BookingId != null).Sum(x => (decimal?)x.Price) ?? 0m,
                PotentialRevenue = g.Sum(x => (decimal?)x.Price) ?? 0m,

                OccupancyNowPercent = g.Count() == 0
                    ? 0m
                    : Math.Round(g.Count(x => x.BookingId != null) * 100m / g.Count(), 2),

                BookingsCountNow = g.Where(x => x.BookingId != null).Select(x => x.BookingId!.Value).Distinct().Count(),

                SeatsSoldPeriod = g.Count(x =>
                    x.BookingId != null
                    && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                    && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value)),

                BookingsCountPeriod = g.Where(x =>
                        x.BookingId != null
                        && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Select(x => x.BookingId!.Value)
                    .Distinct()
                    .Count(),

                RevenuePeriod = g.Where(x =>
                        x.BookingId != null
                        && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Sum(x => (decimal?)x.Price) ?? 0m
            })
            .OrderByDescending(x => x.RevenueNow)
            .Take(50)
            .ToListAsync(ct);

        // --- By Sessions (top 100) ---
        var bySessions = await seatsQ
            .GroupBy(ss => new
            {
                ss.SessionId,
                ss.Session.StartTime,
                MovieTitle = ss.Session.Movie.Title,
                CinemaName = ss.Session.Hall.Cinema.Name,
                HallName = ss.Session.Hall.Name,
                ss.Session.PresentationType
            })
            .Select(g => new StatsBySessionRow
            {
                SessionId = g.Key.SessionId,
                StartTime = g.Key.StartTime,
                MovieTitle = g.Key.MovieTitle,
                CinemaName = g.Key.CinemaName,
                HallName = g.Key.HallName,
                PresentationType = g.Key.PresentationType,

                TotalSeats = g.Count(),
                BookedSeatsNow = g.Count(x => x.BookingId != null),
                RevenueNow = g.Where(x => x.BookingId != null).Sum(x => (decimal?)x.Price) ?? 0m,
                PotentialRevenue = g.Sum(x => (decimal?)x.Price) ?? 0m,

                OccupancyNowPercent = g.Count() == 0
                    ? 0m
                    : Math.Round(g.Count(x => x.BookingId != null) * 100m / g.Count(), 2),

                BookingsCountNow = g.Where(x => x.BookingId != null).Select(x => x.BookingId!.Value).Distinct().Count(),

                SeatsSoldPeriod = g.Count(x =>
                    x.BookingId != null
                    && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                    && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value)),

                BookingsCountPeriod = g.Where(x =>
                        x.BookingId != null
                        && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Select(x => x.BookingId!.Value)
                    .Distinct()
                    .Count(),

                RevenuePeriod = g.Where(x =>
                        x.BookingId != null
                        && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Sum(x => (decimal?)x.Price) ?? 0m
            })
            .OrderByDescending(x => x.RevenueNow)
            .Take(100)
            .ToListAsync(ct);

        // --- By PresentationType ---
        var byPresentation = await seatsQ
            .GroupBy(ss => ss.Session.PresentationType)
            .Select(g => new StatsByPresentationTypeRow
            {
                PresentationType = g.Key,

                SessionsCount = g.Select(x => x.SessionId).Distinct().Count(),
                TotalSeats = g.Count(),

                BookedSeatsNow = g.Count(x => x.BookingId != null),
                RevenueNow = g.Where(x => x.BookingId != null).Sum(x => (decimal?)x.Price) ?? 0m,
                PotentialRevenue = g.Sum(x => (decimal?)x.Price) ?? 0m,

                OccupancyNowPercent = g.Count() == 0
                    ? 0m
                    : Math.Round(g.Count(x => x.BookingId != null) * 100m / g.Count(), 2),

                BookingsCountNow = g.Where(x => x.BookingId != null).Select(x => x.BookingId!.Value).Distinct().Count(),

                SeatsSoldPeriod = g.Count(x =>
                    x.BookingId != null
                    && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                    && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value)),

                BookingsCountPeriod = g.Where(x =>
                        x.BookingId != null
                        && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Select(x => x.BookingId!.Value)
                    .Distinct()
                    .Count(),

                RevenuePeriod = g.Where(x =>
                        x.BookingId != null
                        && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Sum(x => (decimal?)x.Price) ?? 0m
            })
            .OrderByDescending(x => x.RevenueNow)
            .ToListAsync(ct);

        // --- By SeatCategory ---
        var bySeatCategory = await seatsQ
            .GroupBy(ss => ss.Seat.Category)
            .Select(g => new StatsBySeatCategoryRow
            {
                SeatCategory = g.Key,

                SessionsCount = g.Select(x => x.SessionId).Distinct().Count(),
                TotalSeats = g.Count(),

                BookedSeatsNow = g.Count(x => x.BookingId != null),
                RevenueNow = g.Where(x => x.BookingId != null).Sum(x => (decimal?)x.Price) ?? 0m,
                PotentialRevenue = g.Sum(x => (decimal?)x.Price) ?? 0m,

                OccupancyNowPercent = g.Count() == 0
                    ? 0m
                    : Math.Round(g.Count(x => x.BookingId != null) * 100m / g.Count(), 2),

                BookingsCountNow = g.Where(x => x.BookingId != null).Select(x => x.BookingId!.Value).Distinct().Count(),

                SeatsSoldPeriod = g.Count(x =>
                    x.BookingId != null
                    && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                    && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value)),

                BookingsCountPeriod = g.Where(x =>
                        x.BookingId != null
                        && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Select(x => x.BookingId!.Value)
                    .Distinct()
                    .Count(),

                RevenuePeriod = g.Where(x =>
                        x.BookingId != null
                        && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Sum(x => (decimal?)x.Price) ?? 0m
            })
            .OrderByDescending(x => x.RevenueNow)
            .ToListAsync(ct);

        // --- By Day (Session.StartTime.Date) ---
        var bySessionDayRaw = await seatsQ
            .GroupBy(ss => ss.Session.StartTime.Date)
            .Select(g => new
            {
                Day = g.Key,
                SessionsCount = g.Select(x => x.SessionId).Distinct().Count(),
                TotalSeats = g.Count(),
                BookedSeatsNow = g.Count(x => x.BookingId != null),
                RevenueNow = g.Where(x => x.BookingId != null).Sum(x => (decimal?)x.Price) ?? 0m,

                SeatsSoldPeriod = g.Count(x =>
                    x.BookingId != null
                    && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                    && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value)),

                BookingsCountPeriod = g.Where(x =>
                        x.BookingId != null
                        && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Select(x => x.BookingId!.Value)
                    .Distinct()
                    .Count(),

                RevenuePeriod = g.Where(x =>
                        x.BookingId != null
                        && (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Sum(x => (decimal?)x.Price) ?? 0m
            })
            .OrderBy(x => x.Day)
            .ToListAsync(ct);

        var bySessionDay = bySessionDayRaw
            .Select(x => new StatsByDayRow
            {
                Day = DateOnly.FromDateTime(x.Day),
                SessionsCount = x.SessionsCount,
                TotalSeats = x.TotalSeats,
                BookedSeatsNow = x.BookedSeatsNow,
                RevenueNow = x.RevenueNow,
                OccupancyNowPercent = x.TotalSeats == 0 ? 0m : Math.Round(x.BookedSeatsNow * 100m / x.TotalSeats, 2),

                SeatsSoldPeriod = x.SeatsSoldPeriod,
                BookingsCountPeriod = x.BookingsCountPeriod,
                RevenuePeriod = x.RevenuePeriod
            })
            .ToList();

        // --- By Day (Booking.BookedAt.Date) ---
        var byBookingDayRaw = await seatsQ
            .Where(ss => ss.BookingId != null)
            .GroupBy(ss => ss.Booking!.BookedAt.Date)
            .Select(g => new
            {
                Day = g.Key,

                SessionsCount = g.Select(x => x.SessionId).Distinct().Count(),
                SeatsSold = g.Count(),
                Revenue = g.Sum(x => (decimal?)x.Price) ?? 0m,

                SeatsSoldPeriod = g.Count(x =>
                    (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                    && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value)),

                BookingsCountPeriod = g.Where(x =>
                        (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Select(x => x.BookingId!.Value)
                    .Distinct()
                    .Count(),

                RevenuePeriod = g.Where(x =>
                        (!bf.HasValue || x.Booking!.BookedAt >= bf.Value)
                        && (!bt.HasValue || x.Booking!.BookedAt <= bt.Value))
                    .Sum(x => (decimal?)x.Price) ?? 0m
            })
            .OrderBy(x => x.Day)
            .ToListAsync(ct);

        var byBookingDay = byBookingDayRaw
            .Select(x => new StatsByDayRow
            {
                Day = DateOnly.FromDateTime(x.Day),

                // Тут це "продажі по даті бронювання", тому TotalSeats/Occupancy не має сенсу.
                SessionsCount = x.SessionsCount,
                TotalSeats = 0,

                BookedSeatsNow = x.SeatsSold,
                RevenueNow = x.Revenue,
                OccupancyNowPercent = 0m,

                SeatsSoldPeriod = x.SeatsSoldPeriod,
                BookingsCountPeriod = x.BookingsCountPeriod,
                RevenuePeriod = x.RevenuePeriod
            })
            .ToList();

        return new BookingStatisticsResult
        {
            Summary = summary,
            ByCinemas = byCinemas,
            ByHalls = byHalls,
            ByMovies = byMovies,
            BySessions = bySessions,
            ByPresentationTypes = byPresentation,
            BySeatCategories = bySeatCategory,
            BySessionStartDay = bySessionDay,
            ByBookingDay = byBookingDay
        };
    }
}
