using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface IScheduleRepository
{
    Task<Cinema?> GetCinemaAsync(int cinemaId, CancellationToken ct);

    Task<IReadOnlyList<Session>> GetUpcomingByCinemaAsync(
        int cinemaId, DateTime fromInclusive, DateTime toExclusive, CancellationToken ct);
}