using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface ISessionPricingRepository
{
    Task<List<Seat>> GetHallSeatsAsync(int hallId, CancellationToken ct = default);

    Task<List<SessionSeat>> GetSessionSeatsWithSeatAsync(int sessionId, CancellationToken ct = default);

    Task EnsureSessionSeatsCreatedAsync(int sessionId, int hallId, CancellationToken ct = default);

    Task<List<SessionRowPrice>> GetRowPricesAsync(int sessionId, CancellationToken ct = default);
    Task<List<SessionCategoryMultiplier>> GetCategoryMultipliersAsync(int sessionId, CancellationToken ct = default);

    Task ReplaceRowPricesAsync(int sessionId, IEnumerable<SessionRowPrice> rowPrices, CancellationToken ct = default);
    Task ReplaceCategoryMultipliersAsync(int sessionId, IEnumerable<SessionCategoryMultiplier> multipliers, CancellationToken ct = default);

    Task UpdateSessionSeatPricesAsync(int sessionId, IReadOnlyDictionary<int, decimal> seatIdToPrice, CancellationToken ct = default);

    Task<List<int>> GetSessionSeatIdsAsync(int sessionId, CancellationToken ct = default);
    Task AddSessionSeatsAsync(IEnumerable<SessionSeat> seats, CancellationToken ct = default);
}