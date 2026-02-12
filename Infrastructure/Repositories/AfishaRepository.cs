using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AfishaRepository : IAfishaRepository
{
    private readonly CinemaDbContext _db;

    public AfishaRepository(CinemaDbContext db)
    {
        _db = db;
    }

    public async Task<List<Movie>> GetNowShowingAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var windowStart = now.AddDays(-1);

        var windowEnd = now.AddDays(7);

        return await _db.Sessions
                    .AsNoTracking()
                    .Where(s => !s.IsCancelled &&
                                s.StartTime >= windowStart &&
                                s.StartTime <= windowEnd)
                    .Select(s => s.Movie)
                    .Distinct()
                    .OrderByDescending(m => m.ReleaseDate)
                    .ThenBy(m => m.Title)
                    .ToListAsync(ct);
    }

    public async Task<List<Movie>> GetComingSoonAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var todayDate = DateOnly.FromDateTime(now);

        var futureThreshold = now.AddDays(7);

        var activeMovieIds = await _db.Sessions
                    .AsNoTracking()
                    .Where(s => !s.IsCancelled &&
                                s.StartTime >= now.AddDays(-1) &&
                                s.StartTime <= futureThreshold)
                    .Select(s => s.MovieId)
                    .Distinct()
                    .ToListAsync(ct);

        return await _db.Movies
            .AsNoTracking()
            .Where(m => m.ReleaseDate >= todayDate && !activeMovieIds.Contains(m.Id))
            .OrderBy(m => m.ReleaseDate)
            .ThenBy(m => m.Title)
            .ToListAsync(ct);
    }
}
