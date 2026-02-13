using ClosedXML.Excel;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Web.Helpers;

/// <summary>
/// Simple Excel export for Booking statistics.
/// Requires NuGet: ClosedXML
/// </summary>
public static class BookingStatisticsExcelExporter
{
    public static XLWorkbook CreateForAll(BookingStatisticsResult r)
    {
        var wb = new XLWorkbook();
        AddSummary(wb, r);
        AddCinemas(wb, r);
        AddHalls(wb, r);
        AddMovies(wb, r);
        AddSessions(wb, r);
        AddPresentationTypes(wb, r);
        AddSeatCategories(wb, r);
        AddDaysStart(wb, r);
        AddDaysBooked(wb, r);
        return wb;
    }

    public static XLWorkbook CreateForSlice(BookingStatisticsResult r, string slice)
    {
        var wb = new XLWorkbook();
        switch (NormalizeSlice(slice))
        {
            case "summary": AddSummary(wb, r); break;
            case "cinemas": AddCinemas(wb, r); break;
            case "halls": AddHalls(wb, r); break;
            case "movies": AddMovies(wb, r); break;
            case "sessions": AddSessions(wb, r); break;
            case "types": AddPresentationTypes(wb, r); break;
            case "cats": AddSeatCategories(wb, r); break;
            case "days_start": AddDaysStart(wb, r); break;
            case "days_booked": AddDaysBooked(wb, r); break;
            default:
                // щоб не віддавати "порожній" файл
                return CreateForAll(r);
        }
        return wb;
    }

    private static string NormalizeSlice(string? slice)
        => (slice ?? "").Trim().ToLowerInvariant();

    private static string Pt(PresentationType p) => p switch
    {
        PresentationType.TwoD => "2D",
        PresentationType.ThreeD => "3D",
        PresentationType.Imax => "IMAX",
        _ => p.ToString()
    };

    private static string Cat(SeatCategory c) => c switch
    {
        SeatCategory.Standard => "Standard",
        SeatCategory.Vip => "VIP",
        SeatCategory.Accessible => "Accessible",
        _ => c.ToString()
    };

    private static IXLWorksheet Sheet(XLWorkbook wb, string name)
    {
        if (name.Length > 31) name = name[..31];
        return wb.Worksheets.Add(name);
    }

    private static void Header(IXLWorksheet ws, int row, params string[] titles)
    {
        for (int i = 0; i < titles.Length; i++)
            ws.Cell(row, i + 1).Value = titles[i];

        var rng = ws.Range(row, 1, row, titles.Length);
        rng.Style.Font.Bold = true;
        rng.Style.Fill.BackgroundColor = XLColor.LightGray;
    }

    private static void AutoFit(IXLWorksheet ws) => ws.Columns().AdjustToContents();
    private static void Money(IXLWorksheet ws, int col) => ws.Column(col).Style.NumberFormat.Format = "#,##0.00";
    private static void Percent(IXLWorksheet ws, int col) => ws.Column(col).Style.NumberFormat.Format = "0.00";
    private static void Date(IXLWorksheet ws, int col, string format) => ws.Column(col).Style.NumberFormat.Format = format;

    private static void AddSummary(XLWorkbook wb, BookingStatisticsResult r)
    {
        var s = r.Summary;
        var ws = Sheet(wb, "Summary");

        Header(ws, 1, "Metric", "Value");

        var rows = new (string Metric, object Value)[]
        {
            ("GeneratedAt (UTC)", DateTime.UtcNow),

            ("SessionsCount", s.SessionsCount),
            ("ActiveSessionsCount", s.ActiveSessionsCount),
            ("FinishedSessionsCount", s.FinishedSessionsCount),
            ("CancelledSessionsCount", s.CancelledSessionsCount),

            ("TotalSeats", s.TotalSeats),
            ("BookedSeatsNow", s.BookedSeatsNow),
            ("FreeSeatsNow", s.FreeSeatsNow),
            ("OccupancyNowPercent", s.OccupancyNowPercent),

            ("PotentialRevenue", s.PotentialRevenue),
            ("RevenueNow", s.RevenueNow),
            ("RemainingPotentialRevenue", s.RemainingPotentialRevenue),

            ("BookingsCountNow", s.BookingsCountNow),
            ("AverageTicketPriceNow", s.AverageTicketPriceNow),
            ("AverageBookingAmountNow", s.AverageBookingAmountNow),

            ("BookingsCountPeriod", s.BookingsCountPeriod),
            ("SeatsSoldPeriod", s.SeatsSoldPeriod),
            ("RevenuePeriod", s.RevenuePeriod),
            ("AverageTicketPricePeriod", s.AverageTicketPricePeriod),
            ("AverageBookingAmountPeriod", s.AverageBookingAmountPeriod),

            ("CancelledBookingsCountPeriod", s.CancelledBookingsCountPeriod),
            ("CancelledRevenuePeriod", s.CancelledRevenuePeriod),
        };

        int row = 2;
        foreach (var x in rows)
        {
            ws.Cell(row, 1).Value = x.Metric;
            SetCell(ws.Cell(row, 2), x.Value);
            row++;
        }

        AutoFit(ws);
    }

    private static void AddCinemas(XLWorkbook wb, BookingStatisticsResult r)
    {
        var ws = Sheet(wb, "Cinemas");
        Header(ws, 1, "Cinema", "Sessions", "TotalSeats", "BookedNow", "Occupancy%", "RevenueNow", "PotentialRevenue",
            "SeatsSoldPeriod", "BookingsPeriod", "RevenuePeriod");

        int row = 2;
        foreach (var x in r.ByCinemas)
        {
            ws.Cell(row, 1).Value = x.CinemaName;
            ws.Cell(row, 2).Value = x.SessionsCount;
            ws.Cell(row, 3).Value = x.TotalSeats;
            ws.Cell(row, 4).Value = x.BookedSeatsNow;
            ws.Cell(row, 5).Value = x.OccupancyNowPercent;
            ws.Cell(row, 6).Value = x.RevenueNow;
            ws.Cell(row, 7).Value = x.PotentialRevenue;
            ws.Cell(row, 8).Value = x.SeatsSoldPeriod;
            ws.Cell(row, 9).Value = x.BookingsCountPeriod;
            ws.Cell(row, 10).Value = x.RevenuePeriod;
            row++;
        }

        Percent(ws, 5);
        Money(ws, 6);
        Money(ws, 7);
        Money(ws, 10);
        AutoFit(ws);
    }

    private static void AddHalls(XLWorkbook wb, BookingStatisticsResult r)
    {
        var ws = Sheet(wb, "Halls");
        Header(ws, 1, "Cinema", "Hall", "Sessions", "TotalSeats", "BookedNow", "Occupancy%", "RevenueNow",
            "PotentialRevenue", "SeatsSoldPeriod", "BookingsPeriod", "RevenuePeriod");

        int row = 2;
        foreach (var x in r.ByHalls)
        {
            ws.Cell(row, 1).Value = x.CinemaName;
            ws.Cell(row, 2).Value = x.HallName;
            ws.Cell(row, 3).Value = x.SessionsCount;
            ws.Cell(row, 4).Value = x.TotalSeats;
            ws.Cell(row, 5).Value = x.BookedSeatsNow;
            ws.Cell(row, 6).Value = x.OccupancyNowPercent;
            ws.Cell(row, 7).Value = x.RevenueNow;
            ws.Cell(row, 8).Value = x.PotentialRevenue;
            ws.Cell(row, 9).Value = x.SeatsSoldPeriod;
            ws.Cell(row, 10).Value = x.BookingsCountPeriod;
            ws.Cell(row, 11).Value = x.RevenuePeriod;
            row++;
        }

        Percent(ws, 6);
        Money(ws, 7);
        Money(ws, 8);
        Money(ws, 11);
        AutoFit(ws);
    }

    private static void AddMovies(XLWorkbook wb, BookingStatisticsResult r)
    {
        var ws = Sheet(wb, "Movies");
        Header(ws, 1, "Movie", "Sessions", "TotalSeats", "BookedNow", "Occupancy%", "RevenueNow", "PotentialRevenue",
            "SeatsSoldPeriod", "BookingsPeriod", "RevenuePeriod");

        int row = 2;
        foreach (var x in r.ByMovies)
        {
            ws.Cell(row, 1).Value = x.MovieTitle;
            ws.Cell(row, 2).Value = x.SessionsCount;
            ws.Cell(row, 3).Value = x.TotalSeats;
            ws.Cell(row, 4).Value = x.BookedSeatsNow;
            ws.Cell(row, 5).Value = x.OccupancyNowPercent;
            ws.Cell(row, 6).Value = x.RevenueNow;
            ws.Cell(row, 7).Value = x.PotentialRevenue;
            ws.Cell(row, 8).Value = x.SeatsSoldPeriod;
            ws.Cell(row, 9).Value = x.BookingsCountPeriod;
            ws.Cell(row, 10).Value = x.RevenuePeriod;
            row++;
        }

        Percent(ws, 5);
        Money(ws, 6);
        Money(ws, 7);
        Money(ws, 10);
        AutoFit(ws);
    }

    private static void AddSessions(XLWorkbook wb, BookingStatisticsResult r)
    {
        var ws = Sheet(wb, "Sessions");
        Header(ws, 1, "SessionId", "StartTime", "Movie", "Cinema", "Hall", "Type",
            "TotalSeats", "BookedNow", "Occupancy%", "RevenueNow", "PotentialRevenue",
            "SeatsSoldPeriod", "BookingsPeriod", "RevenuePeriod");

        int row = 2;
        foreach (var x in r.BySessions)
        {
            ws.Cell(row, 1).Value = x.SessionId;
            ws.Cell(row, 2).Value = x.StartTime;
            ws.Cell(row, 3).Value = x.MovieTitle;
            ws.Cell(row, 4).Value = x.CinemaName;
            ws.Cell(row, 5).Value = x.HallName;
            ws.Cell(row, 6).Value = Pt(x.PresentationType);
            ws.Cell(row, 7).Value = x.TotalSeats;
            ws.Cell(row, 8).Value = x.BookedSeatsNow;
            ws.Cell(row, 9).Value = x.OccupancyNowPercent;
            ws.Cell(row, 10).Value = x.RevenueNow;
            ws.Cell(row, 11).Value = x.PotentialRevenue;
            ws.Cell(row, 12).Value = x.SeatsSoldPeriod;
            ws.Cell(row, 13).Value = x.BookingsCountPeriod;
            ws.Cell(row, 14).Value = x.RevenuePeriod;
            row++;
        }

        Date(ws, 2, "dd.mm.yyyy hh:mm");
        Percent(ws, 9);
        Money(ws, 10);
        Money(ws, 11);
        Money(ws, 14);
        AutoFit(ws);
    }

    private static void AddPresentationTypes(XLWorkbook wb, BookingStatisticsResult r)
    {
        var ws = Sheet(wb, "Types");
        Header(ws, 1, "PresentationType", "Sessions", "TotalSeats", "BookedNow", "Occupancy%",
            "RevenueNow", "PotentialRevenue", "SeatsSoldPeriod", "BookingsPeriod", "RevenuePeriod");

        int row = 2;
        foreach (var x in r.ByPresentationTypes)
        {
            ws.Cell(row, 1).Value = Pt(x.PresentationType);
            ws.Cell(row, 2).Value = x.SessionsCount;
            ws.Cell(row, 3).Value = x.TotalSeats;
            ws.Cell(row, 4).Value = x.BookedSeatsNow;
            ws.Cell(row, 5).Value = x.OccupancyNowPercent;
            ws.Cell(row, 6).Value = x.RevenueNow;
            ws.Cell(row, 7).Value = x.PotentialRevenue;
            ws.Cell(row, 8).Value = x.SeatsSoldPeriod;
            ws.Cell(row, 9).Value = x.BookingsCountPeriod;
            ws.Cell(row, 10).Value = x.RevenuePeriod;
            row++;
        }

        Percent(ws, 5);
        Money(ws, 6);
        Money(ws, 7);
        Money(ws, 10);
        AutoFit(ws);
    }

    private static void AddSeatCategories(XLWorkbook wb, BookingStatisticsResult r)
    {
        var ws = Sheet(wb, "SeatCats");
        Header(ws, 1, "SeatCategory", "Sessions", "TotalSeats", "BookedNow", "Occupancy%",
            "RevenueNow", "PotentialRevenue", "SeatsSoldPeriod", "BookingsPeriod", "RevenuePeriod");

        int row = 2;
        foreach (var x in r.BySeatCategories)
        {
            ws.Cell(row, 1).Value = Cat(x.SeatCategory);
            ws.Cell(row, 2).Value = x.SessionsCount;
            ws.Cell(row, 3).Value = x.TotalSeats;
            ws.Cell(row, 4).Value = x.BookedSeatsNow;
            ws.Cell(row, 5).Value = x.OccupancyNowPercent;
            ws.Cell(row, 6).Value = x.RevenueNow;
            ws.Cell(row, 7).Value = x.PotentialRevenue;
            ws.Cell(row, 8).Value = x.SeatsSoldPeriod;
            ws.Cell(row, 9).Value = x.BookingsCountPeriod;
            ws.Cell(row, 10).Value = x.RevenuePeriod;
            row++;
        }

        Percent(ws, 5);
        Money(ws, 6);
        Money(ws, 7);
        Money(ws, 10);
        AutoFit(ws);
    }

    private static void AddDaysStart(XLWorkbook wb, BookingStatisticsResult r)
    {
        var ws = Sheet(wb, "Days(Start)");
        Header(ws, 1, "Day", "Sessions", "TotalSeats", "BookedNow", "Occupancy%",
            "RevenueNow", "SeatsSoldPeriod", "BookingsPeriod", "RevenuePeriod");

        int row = 2;
        foreach (var x in r.BySessionStartDay)
        {
            ws.Cell(row, 1).Value = x.Day.ToDateTime(TimeOnly.MinValue);
            ws.Cell(row, 2).Value = x.SessionsCount;
            ws.Cell(row, 3).Value = x.TotalSeats;
            ws.Cell(row, 4).Value = x.BookedSeatsNow;
            ws.Cell(row, 5).Value = x.OccupancyNowPercent;
            ws.Cell(row, 6).Value = x.RevenueNow;
            ws.Cell(row, 7).Value = x.SeatsSoldPeriod;
            ws.Cell(row, 8).Value = x.BookingsCountPeriod;
            ws.Cell(row, 9).Value = x.RevenuePeriod;
            row++;
        }

        Date(ws, 1, "dd.mm.yyyy");
        Percent(ws, 5);
        Money(ws, 6);
        Money(ws, 9);
        AutoFit(ws);
    }

    private static void AddDaysBooked(XLWorkbook wb, BookingStatisticsResult r)
    {
        var ws = Sheet(wb, "Days(Booked)");
        Header(ws, 1, "Day", "Sessions", "SeatsSold (all)", "Revenue (all)",
            "SeatsSoldPeriod", "BookingsPeriod", "RevenuePeriod");

        int row = 2;
        foreach (var x in r.ByBookingDay)
        {
            ws.Cell(row, 1).Value = x.Day.ToDateTime(TimeOnly.MinValue);
            ws.Cell(row, 2).Value = x.SessionsCount;
            ws.Cell(row, 3).Value = x.BookedSeatsNow; // для ByBookingDay це SeatsSold (all)
            ws.Cell(row, 4).Value = x.RevenueNow;     // для ByBookingDay це Revenue (all)
            ws.Cell(row, 5).Value = x.SeatsSoldPeriod;
            ws.Cell(row, 6).Value = x.BookingsCountPeriod;
            ws.Cell(row, 7).Value = x.RevenuePeriod;
            row++;
        }

        Date(ws, 1, "dd.mm.yyyy");
        Money(ws, 4);
        Money(ws, 7);
        AutoFit(ws);
    }

    private static void SetCell(IXLCell cell, object? value)
    {
        if (value is null)
        {
            cell.Clear();
            return;
        }

        switch (value)
        {
            case string s:
                cell.Value = s;
                break;

            case DateTime dt:
                cell.Value = dt;
                break;

            case DateTimeOffset dto:
                cell.Value = dto.UtcDateTime;
                break;

            case bool b:
                cell.Value = b;
                break;

            case int i:
                cell.Value = i;
                break;

            case long l:
                cell.Value = l;
                break;

            case double d:
                cell.Value = d;
                break;

            case float f:
                cell.Value = (double)f;
                break;

            case decimal m:
                cell.Value = (double)m;
                break;

            default:
                // на крайняк — текстом
                cell.Value = value.ToString() ?? "";
                break;
        }
    }

}
