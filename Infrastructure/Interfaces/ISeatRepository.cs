using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface ISeatRepository
{
    Task<bool> AnyForHallAsync(int hallId);
    Task<List<Seat>> GetByHallAsync(int hallId);

    Task AddRangeAsync(IEnumerable<Seat> seats);

    Task DeleteByHallAsync(int hallId);
}
