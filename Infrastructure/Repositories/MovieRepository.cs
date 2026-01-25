using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly CinemaDbContext _db;

    public MovieRepository(CinemaDbContext db) => _db = db;

    public async Task<(IEnumerable<Movie> Items, int TotalCount)> GetAllAsync(
        string? searchTerm,
        string? sortBy,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _db.Movies
            .AsNoTracking()
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .Include(m => m.Sessions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(m => EF.Functions.Like(m.Title, $"%{term}%"));
        }

        query = (sortBy ?? "title").ToLowerInvariant() switch
        {
            "date_asc" => query.OrderBy(m => m.ReleaseDate),
            "date_desc" => query.OrderByDescending(m => m.ReleaseDate),
            "title_desc" => query.OrderByDescending(m => m.Title),
            _ => query.OrderBy(m => m.Title)
        };

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task<Movie?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Movies
            .AsNoTracking()
            .Include(m => m.MovieGenres)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<Movie?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
        _db.Movies
            .AsNoTracking()
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .Include(m => m.Sessions)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task AddAsync(Movie movie, IEnumerable<int> genreIds, CancellationToken ct = default)
    {
        var ids = (genreIds ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToList();
        foreach (var gId in ids)
            movie.MovieGenres.Add(new MovieGenre { GenreId = gId });

        await _db.Movies.AddAsync(movie, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Movie movie, IEnumerable<int> genreIds, CancellationToken ct = default)
    {
        var existing = await _db.Movies
            .Include(m => m.MovieGenres)
            .FirstOrDefaultAsync(m => m.Id == movie.Id, ct);

        if (existing == null)
            throw new InvalidOperationException("Movie not found.");

        existing.Title = movie.Title;
        existing.Description = movie.Description;
        existing.ReleaseDate = movie.ReleaseDate;
        existing.Duration = movie.Duration;
        existing.ProductionCountryCode = movie.ProductionCountryCode;

        existing.MovieGenres.Clear();
        var ids = (genreIds ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToList();
        foreach (var gId in ids)
            existing.MovieGenres.Add(new MovieGenre { MovieId = existing.Id, GenreId = gId });

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var movie = await _db.Movies.FindAsync([id], ct);
        if (movie == null) return;

        _db.Movies.Remove(movie);
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> AnySessionsAsync(int movieId, CancellationToken ct = default) =>
        _db.Sessions.AsNoTracking().AnyAsync(s => s.MovieId == movieId, ct);
}
