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
        return await _db.Sessions
            .AsNoTracking()
            .Where(s => !s.IsCancelled)
            .Select(s => s.Movie)
            .Distinct()
            .OrderBy(m => m.Title)
            .ToListAsync(ct);
    }

    public async Task<List<Movie>> GetComingSoonAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var nowIds = await _db.Sessions
            .AsNoTracking()
            .Where(s => !s.IsCancelled)
            .Select(s => s.MovieId)
            .Distinct()
            .ToListAsync(ct);

        return await _db.Movies
            .AsNoTracking()
            .Where(m => !nowIds.Contains(m.Id) && m.ReleaseDate > today)
            .OrderBy(m => m.Title)
            .ToListAsync(ct);
    }
}
