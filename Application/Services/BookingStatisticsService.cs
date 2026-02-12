using Application.Interfaces;
using Infrastructure.Interfaces;

namespace Application.Services;

public class BookingStatisticsService : IBookingStatisticsService
{
    private readonly IBookingStatisticsRepository _repo;

    public BookingStatisticsService(IBookingStatisticsRepository repo)
    {
        _repo = repo;
    }

    public Task<BookingStatisticsResult> GetBookingStatisticsAsync(BookingStatisticsFilter filter, CancellationToken ct)
        => _repo.GetBookingStatisticsAsync(filter, ct);
}
