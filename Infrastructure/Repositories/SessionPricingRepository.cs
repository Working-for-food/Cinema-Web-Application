using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class SessionPricingRepository : ISessionPricingRepository
{
    private readonly CinemaDbContext _db;
    public SessionPricingRepository(CinemaDbContext db) => _db = db;

    public Task<List<Seat>> GetHallSeatsAsync(int hallId, CancellationToken ct = default)
        => _db.Seats
            .AsNoTracking()
            .Where(s => s.HallId == hallId)
            .OrderBy(s => s.RowNumber)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync(ct);

    public Task<List<SessionSeat>> GetSessionSeatsWithSeatAsync(int sessionId, CancellationToken ct = default)
        => _db.SessionSeats
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .Include(x => x.Seat)
            .OrderBy(x => x.Seat.RowNumber)
            .ThenBy(x => x.Seat.SeatNumber)
            .ToListAsync(ct);

    public async Task EnsureSessionSeatsCreatedAsync(int sessionId, int hallId, CancellationToken ct = default)
    {
        var hallSeatIds = await _db.Seats
            .AsNoTracking()
            .Where(s => s.HallId == hallId)
            .Select(s => s.Id)
            .ToListAsync(ct);

        if (hallSeatIds.Count == 0) return;

        var existingSeatIds = await _db.SessionSeats
            .AsNoTracking()
            .Where(ss => ss.SessionId == sessionId)
            .Select(ss => ss.SeatId)
            .ToListAsync(ct);

        var missing = hallSeatIds.Except(existingSeatIds).ToList();
        if (missing.Count == 0) return;

        var items = missing.Select(seatId => new SessionSeat
        {
            SessionId = sessionId,
            SeatId = seatId,
            Price = 0m
        });

        _db.SessionSeats.AddRange(items);
        await _db.SaveChangesAsync(ct);
    }

    public Task<List<SessionRowPrice>> GetRowPricesAsync(int sessionId, CancellationToken ct = default)
        => _db.Set<SessionRowPrice>()
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .OrderBy(x => x.RowNumber)
            .ToListAsync(ct);

    public Task<List<SessionCategoryMultiplier>> GetCategoryMultipliersAsync(int sessionId, CancellationToken ct = default)
        => _db.Set<SessionCategoryMultiplier>()
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .OrderBy(x => x.Category)
            .ToListAsync(ct);

    public async Task ReplaceRowPricesAsync(int sessionId, IEnumerable<SessionRowPrice> rowPrices, CancellationToken ct = default)
    {
        var old = await _db.Set<SessionRowPrice>()
            .Where(x => x.SessionId == sessionId)
            .ToListAsync(ct);

        _db.RemoveRange(old);
        _db.AddRange(rowPrices);

        await _db.SaveChangesAsync(ct);
    }

    public async Task ReplaceCategoryMultipliersAsync(int sessionId, IEnumerable<SessionCategoryMultiplier> multipliers, CancellationToken ct = default)
    {
        var old = await _db.Set<SessionCategoryMultiplier>()
            .Where(x => x.SessionId == sessionId)
            .ToListAsync(ct);

        _db.RemoveRange(old);
        _db.AddRange(multipliers);

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateSessionSeatPricesAsync(int sessionId, IReadOnlyDictionary<int, decimal> seatIdToPrice, CancellationToken ct = default)
    {
        if (seatIdToPrice.Count == 0) return;

        var seatIds = seatIdToPrice.Keys.ToList();

        var items = await _db.SessionSeats
            .Where(x => x.SessionId == sessionId && seatIds.Contains(x.SeatId))
            .ToListAsync(ct);

        foreach (var ss in items)
        {
            if (seatIdToPrice.TryGetValue(ss.SeatId, out var price))
                ss.Price = price;
        }

        await _db.SaveChangesAsync(ct);
    }

    public Task<List<int>> GetSessionSeatIdsAsync(int sessionId, CancellationToken ct = default)
        => _db.SessionSeats
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .Select(x => x.SeatId)
            .ToListAsync(ct);

    public async Task AddSessionSeatsAsync(IEnumerable<SessionSeat> seats, CancellationToken ct = default)
    {
        _db.SessionSeats.AddRange(seats);
        await _db.SaveChangesAsync(ct);
    }
}
