using Infrastructure.Interfaces;

namespace Application.Interfaces;

public interface IBookingStatisticsService
{
    Task<BookingStatisticsResult> GetBookingStatisticsAsync(BookingStatisticsFilter filter, CancellationToken ct);
}
