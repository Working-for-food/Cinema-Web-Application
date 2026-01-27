using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SeatRepository : ISeatRepository
{
    private readonly CinemaDbContext _db;

    public SeatRepository(CinemaDbContext db) => _db = db;

    public Task<bool> AnyForHallAsync(int hallId)
        => _db.Seats.AnyAsync(s => s.HallId == hallId);

    public Task<List<Seat>> GetByHallAsync(int hallId)
        => _db.Seats
            .Where(s => s.HallId == hallId)
            .AsNoTracking()
            .OrderBy(s => s.RowNumber).ThenBy(s => s.SeatNumber)
            .ToListAsync();

    public async Task AddRangeAsync(IEnumerable<Seat> seats)
    {
        _db.Seats.AddRange(seats);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteByHallAsync(int hallId)
    {
        await _db.Seats.Where(s => s.HallId == hallId).ExecuteDeleteAsync();
    }

    public async Task<Dictionary<int, int>> CountByHallIdsAsync(IEnumerable<int> hallIds)
    {
        var ids = hallIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<int, int>();

        return await _db.Seats
            .Where(s => ids.Contains(s.HallId))
            .GroupBy(s => s.HallId)
            .Select(g => new { HallId = g.Key, Cnt = g.Count() })
            .ToDictionaryAsync(x => x.HallId, x => x.Cnt);
    }
}
