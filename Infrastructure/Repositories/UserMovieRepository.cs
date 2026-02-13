using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserMovieRepository : IUserMovieRepository
{
    private readonly CinemaDbContext _db;

    public UserMovieRepository(CinemaDbContext db)
    {
        _db = db;
    }
    public async Task<IReadOnlyList<Movie>> GetRelatedMoviesAsync(
    int movieId,
    IReadOnlyList<string> genres,
    int take,
    DateTime nowUtc,
    CancellationToken ct = default)
    {
        var baseQuery = _db.Movies
            .AsNoTracking()
            .Where(m =>
                !m.IsDeleted &&
                m.Id != movieId &&
                m.Sessions.Any(s => !s.IsCancelled && s.StartTime >= nowUtc));

        List<Movie> byGenre = new();

        if (genres is { Count: > 0 })
        {
            byGenre = await baseQuery
                .Where(m => m.MovieGenres.Any(mg => genres.Contains(mg.Genre.Name)))
                .OrderBy(m => m.Title)
                .Select(m => new Movie
                {
                    Id = m.Id,
                    Title = m.Title,
                    PosterPath = m.PosterPath
                })
                .Take(take)
                .ToListAsync(ct);
        }

        if (byGenre.Count >= take)
            return byGenre;

        var usedIds = byGenre.Select(x => x.Id).ToList();

        var fallback = await baseQuery
            .Where(m => !usedIds.Contains(m.Id))
            .OrderBy(m => m.Title)
            .Select(m => new Movie
            {
                Id = m.Id,
                Title = m.Title,
                PosterPath = m.PosterPath
            })
            .Take(take - byGenre.Count)
            .ToListAsync(ct);

        return byGenre.Concat(fallback).ToList();
    }

    public Task<Movie?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return _db.Movies
            .AsNoTracking()
            .Include(m => m.MovieDirectors)
                .ThenInclude(md => md.DirectorId)
            .Include(m => m.MovieCountries)
                .ThenInclude(mc => mc.Country)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }
    public Task<Movie?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
    _db.Movies
        .AsNoTracking()
        .Where(m => m.Id == id && !m.IsDeleted)
        .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
        .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
        .Include(m => m.MovieDirectors).ThenInclude(md => md.Director)
        .Include(m => m.MovieCountries).ThenInclude(mc => mc.Country)
        .Include(m => m.Sessions)
            .ThenInclude(s => s.Hall)
                .ThenInclude(h => h.Cinema)
        .FirstOrDefaultAsync(ct);
}
