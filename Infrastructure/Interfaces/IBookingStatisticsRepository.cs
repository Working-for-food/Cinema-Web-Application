namespace Infrastructure.Interfaces;

public interface IBookingStatisticsRepository
{
    Task<BookingStatisticsResult> GetBookingStatisticsAsync(BookingStatisticsFilter filter, CancellationToken ct);
}
