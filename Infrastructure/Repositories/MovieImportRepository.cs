using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Interfaces;

namespace Infrastructure.Repositories;

public sealed class MovieImportRepository : IMovieImportRepository
{
    private readonly CinemaDbContext _db;

    public MovieImportRepository(CinemaDbContext db) => _db = db;

    public Task<Movie?> GetMovieByTmdbIdAsync(int tmdbId, CancellationToken ct) =>
        _db.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId, ct);

    public Task<Movie?> GetMovieByIdAsync(int id, CancellationToken ct) =>
        _db.Movies.FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task AddMovieAsync(Movie movie, CancellationToken ct)
    {
        _db.Movies.Add(movie);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    public async Task<Genre> UpsertGenreByTmdbAsync(int tmdbGenreId, string name, CancellationToken ct)
    {
        name = NormalizeWs(name);
        if (string.IsNullOrWhiteSpace(name)) name = "—";
        if (name.Length > 120) name = name[..120];

        var genre = await _db.Genres.FirstOrDefaultAsync(g => g.TmdbId == tmdbGenreId, ct);
        if (genre != null)
        {
            if (!string.Equals(genre.Name, name, StringComparison.OrdinalIgnoreCase))
                genre.Name = name;

            await _db.SaveChangesAsync(ct);
            return genre;
        }

        var byName = await _db.Genres.FirstOrDefaultAsync(g => g.Name == name, ct);
        if (byName != null)
        {
            if (!byName.TmdbId.HasValue)
                byName.TmdbId = tmdbGenreId;

            if (byName.TmdbId != tmdbGenreId)
                return byName;

            await _db.SaveChangesAsync(ct);
            return byName;
        }

        genre = new Genre { TmdbId = tmdbGenreId, Name = name };
        _db.Genres.Add(genre);
        await _db.SaveChangesAsync(ct);
        return genre;
    }


    public async Task<Person> UpsertPersonByTmdbAsync(int tmdbPersonId, string fullName, string? photoPath, CancellationToken ct)
    {
        fullName = NormalizeWs(fullName);
        if (string.IsNullOrWhiteSpace(fullName)) fullName = "Unknown";
        if (fullName.Length > 180) fullName = fullName[..180];

        photoPath = NormalizeNullable(photoPath, 700);

        var person = await _db.People.FirstOrDefaultAsync(p => p.TmdbId == tmdbPersonId, ct);
        if (person != null)
        {
            if (!string.IsNullOrWhiteSpace(fullName))
                person.FullName = fullName;

            if (!string.IsNullOrWhiteSpace(photoPath))
                person.PhotoUrl = photoPath;

            person.TmdbLastSyncAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
            return person;
        }

        var manual = await _db.People.FirstOrDefaultAsync(p => p.TmdbId == null && p.FullName == fullName, ct);
        if (manual != null)
        {
            manual.TmdbId = tmdbPersonId;
            if (!string.IsNullOrWhiteSpace(photoPath))
                manual.PhotoUrl = photoPath;

            manual.TmdbLastSyncAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
            return manual;
        }

        person = new Person
        {
            TmdbId = tmdbPersonId,
            FullName = fullName,
            PhotoUrl = photoPath,
            TmdbLastSyncAt = DateTimeOffset.UtcNow
        };

        _db.People.Add(person);
        await _db.SaveChangesAsync(ct);
        return person;
    }


    public async Task<IReadOnlyList<string>> FilterExistingCountryCodesAsync(IReadOnlyList<string> codes, CancellationToken ct) =>
        await _db.Countries.Where(c => codes.Contains(c.Code)).Select(c => c.Code).ToListAsync(ct);

    public async Task ReplaceMovieGenresAsync(int movieId, IReadOnlyList<int> genreIds, CancellationToken ct)
    {
        var old = await _db.MovieGenres.Where(x => x.MovieId == movieId).ToListAsync(ct);
        _db.MovieGenres.RemoveRange(old);

        _db.MovieGenres.AddRange(genreIds.Distinct().Select(id => new MovieGenre { MovieId = movieId, GenreId = id }));
    }

    public async Task ReplaceMovieCountriesAsync(int movieId, IReadOnlyList<string> countryCodes, CancellationToken ct)
    {
        var old = await _db.MovieCountries.Where(x => x.MovieId == movieId).ToListAsync(ct);
        _db.MovieCountries.RemoveRange(old);

        _db.MovieCountries.AddRange(countryCodes.Distinct().Select(code => new MovieCountry { MovieId = movieId, CountryCode = code }));
    }

    public async Task ReplaceMovieActorsAsync(int movieId, IReadOnlyList<(Person person, short order, string? character)> actors, CancellationToken ct)
    {
        var old = await _db.MovieActors.Where(x => x.MovieId == movieId).ToListAsync(ct);
        _db.MovieActors.RemoveRange(old);

        var unique = actors
            .Where(a => a.person != null)
            .GroupBy(a => a.person.Id)
            .Select(g => g.OrderBy(x => x.order).First())
            .OrderBy(x => x.order)
            .ToList();

        short cust = 1;
<<<<<<< HEAD
        foreach (var a in sorted)
=======
        foreach (var a in unique)
>>>>>>> feature/auth
        {
            _db.MovieActors.Add(new MovieActor
            {
                MovieId = movieId,
                ActorId = a.person.Id,
                CustOrder = cust++,
                CharacterName = string.IsNullOrWhiteSpace(a.character) ? null : a.character.Trim()
            });
        }
    }


    public async Task ReplaceMovieDirectorsAsync(int movieId, IReadOnlyList<(Person person, short order)> directors, CancellationToken ct)
    {
        var old = await _db.MovieDirectors.Where(x => x.MovieId == movieId).ToListAsync(ct);
        _db.MovieDirectors.RemoveRange(old);
<<<<<<< HEAD
        var sorted = directors.OrderBy(a => a.order).ToList();
        short cust = 1;
        foreach (var d in sorted)
=======

        var unique = directors
            .Where(d => d.person != null)
            .GroupBy(d => d.person.Id)
            .Select(g => g.OrderBy(x => x.order).First())
            .OrderBy(x => x.order)
            .ToList();

        short billing = 1;
        foreach (var d in unique)
>>>>>>> feature/auth
        {
            _db.MovieDirectors.Add(new MovieDirector
            {
                MovieId = movieId,
                DirectorId = d.person.Id,
<<<<<<< HEAD
                BillingOrder = cust++
=======
                BillingOrder = billing++
>>>>>>> feature/auth
            });
        }
    }

    private static string NormalizeWs(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        return string.Join(' ', s.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }

    private static string? NormalizeNullable(string? s, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var x = NormalizeWs(s);
        return x.Length <= maxLen ? x : x[..maxLen];
    }


}
