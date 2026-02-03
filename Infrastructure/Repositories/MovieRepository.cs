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
        .Include(m => m.MovieActors)
        .Include(m => m.MovieCountries)
        .Include(m => m.MovieDirectors)
        .FirstOrDefaultAsync(m => m.Id == id, ct);


    public Task<Movie?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
    _db.Movies
        .AsNoTracking()
        .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
        .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
        .Include(m => m.MovieCountries).ThenInclude(mc => mc.Country)
        .Include(m => m.MovieDirectors).ThenInclude(md => md.Director)
        .Include(m => m.Sessions)
        .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<string?> GetTitleByIdAsync(int id, CancellationToken ct)
    {
        return await _db.Movies
            .Where(m => m.Id == id)
            .Select(m => m.Title)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(
    Movie movie,
    IEnumerable<int> genreIds,
    IEnumerable<int> actorIds,
    IEnumerable<string> countryCodes,
    IEnumerable<int> directorIds,
    CancellationToken ct = default)
    {
        var gIds = (genreIds ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToList();
        foreach (var gId in gIds)
            movie.MovieGenres.Add(new MovieGenre { GenreId = gId });

        var aIds = (actorIds ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToList();
        short actorOrder = 1;
        foreach (var aId in aIds)
            movie.MovieActors.Add(new MovieActor { ActorId = aId, CustOrder = actorOrder++ });

        var codes = (countryCodes ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Where(x => x.Length == 2)
            .Distinct()
            .ToList();
        foreach (var code in codes)
            movie.MovieCountries.Add(new MovieCountry { CountryCode = code });

        var dIds = (directorIds ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToList();
        short billing = 1;
        foreach (var dId in dIds)
            movie.MovieDirectors.Add(new MovieDirector { DirectorId = dId, BillingOrder = billing++ });

        await _db.Movies.AddAsync(movie, ct);
        await _db.SaveChangesAsync(ct);
    }


    public async Task UpdateAsync(
    Movie movie,
    IEnumerable<int> genreIds,
    IEnumerable<int> actorIds,
    IEnumerable<string> countryCodes,
    IEnumerable<int> directorIds,
    CancellationToken ct = default)
    {
        var existing = await _db.Movies
            .Include(m => m.MovieGenres)
            .Include(m => m.MovieActors)
            .Include(m => m.MovieCountries)
            .Include(m => m.MovieDirectors)
            .FirstOrDefaultAsync(m => m.Id == movie.Id, ct);

        if (existing == null)
            throw new InvalidOperationException("Movie not found.");

        existing.Title = movie.Title;
        existing.Description = movie.Description;
        existing.ReleaseDate = movie.ReleaseDate;
        existing.Duration = movie.Duration;
        existing.PosterPath = movie.PosterPath;
        existing.BackdropPath = movie.BackdropPath;
        existing.OriginalName = movie.OriginalName;
        existing.Language = movie.Language;
        existing.TrailerUrl = movie.TrailerUrl;

        // genres
        existing.MovieGenres.Clear();
        var gIds = (genreIds ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToList();
        foreach (var gId in gIds)
            existing.MovieGenres.Add(new MovieGenre { MovieId = existing.Id, GenreId = gId });

        // actors
        var aIds = (actorIds ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToList();
        var existingByActorId = existing.MovieActors.ToDictionary(x => x.ActorId, x => x);
        var toRemove = existing.MovieActors.Where(x => !aIds.Contains(x.ActorId)).ToList();
        _db.MovieActors.RemoveRange(toRemove);
        short next = (short)((existing.MovieActors.Count == 0) ? 1 : (existing.MovieActors.Max(x => x.CustOrder) + 1));
        foreach (var aId in aIds)
        {
            if (existingByActorId.ContainsKey(aId)) continue;
            existing.MovieActors.Add(new MovieActor { MovieId = existing.Id, ActorId = aId, CustOrder = next++ });
        }

        // countries
        existing.MovieCountries.Clear();
        var codes = (countryCodes ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Where(x => x.Length == 2)
            .Distinct()
            .ToList();
        foreach (var code in codes)
            existing.MovieCountries.Add(new MovieCountry { MovieId = existing.Id, CountryCode = code });

        // directors
        var dIds = (directorIds ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToList();
        var existingByDirectorId = existing.MovieDirectors.ToDictionary(x => x.DirectorId, x => x);
        var dToRemove = existing.MovieDirectors.Where(x => !dIds.Contains(x.DirectorId)).ToList();
        _db.MovieDirectors.RemoveRange(dToRemove);
        short nextBilling = (short)((existing.MovieDirectors.Count == 0) ? 1 : (existing.MovieDirectors.Max(x => x.BillingOrder) + 1));
        foreach (var dId in dIds)
        {
            if (existingByDirectorId.ContainsKey(dId)) continue;
            existing.MovieDirectors.Add(new MovieDirector { MovieId = existing.Id, DirectorId = dId, BillingOrder = nextBilling++ });
        }    

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

    public Task<List<Movie>> SearchAsync(string? query, int take, CancellationToken ct)
    {
        var q = _db.Movies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(m => m.Title.Contains(query));

        return q.OrderBy(m => m.Title)
                .Take(take)
                .ToListAsync(ct);
    }
}
