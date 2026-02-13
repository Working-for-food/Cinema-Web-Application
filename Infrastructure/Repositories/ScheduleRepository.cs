using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ScheduleRepository : IScheduleRepository
{
    private readonly CinemaDbContext _db;
    public ScheduleRepository(CinemaDbContext db) => _db = db;

    public Task<Cinema?> GetCinemaAsync(int cinemaId, CancellationToken ct)
        => _db.Cinemas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cinemaId, ct);

    public async Task<IReadOnlyList<Session>> GetUpcomingByCinemaAsync(
        int cinemaId,
        DateTime fromInclusive,
        DateTime toExclusive,
        CancellationToken ct)
    {
        return await _db.Sessions
            .AsNoTracking()
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => !s.IsCancelled)
            .Where(s => s.Hall.CinemaId == cinemaId)
            .Where(s => s.StartTime >= fromInclusive && s.StartTime < toExclusive)
            .OrderBy(s => s.StartTime)
            .ToListAsync(ct);
    }
}
