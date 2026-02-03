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
        var genre = await _db.Genres.FirstOrDefaultAsync(g => g.TmdbId == tmdbGenreId, ct);
        if (genre is null)
        {
            genre = new Genre { TmdbId = tmdbGenreId, Name = name };
            _db.Genres.Add(genre);
        }
        else if (!string.IsNullOrWhiteSpace(name))
        {
            genre.Name = name;
        }

        await _db.SaveChangesAsync(ct);
        return genre;
    }

    public async Task<Person> UpsertPersonByTmdbAsync(int tmdbPersonId, string fullName, string? photoPath, CancellationToken ct)
    {
        var person = await _db.People.FirstOrDefaultAsync(p => p.TmdbId == tmdbPersonId, ct);
        if (person is null)
        {
            person = new Person
            {
                TmdbId = tmdbPersonId,
                FullName = string.IsNullOrWhiteSpace(fullName) ? "Unknown" : fullName,
                PhotoUrl = photoPath,
                TmdbLastSyncAt = DateTimeOffset.UtcNow
            };
            _db.People.Add(person);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(fullName)) person.FullName = fullName;
            if (!string.IsNullOrWhiteSpace(photoPath)) person.PhotoUrl = photoPath;
            person.TmdbLastSyncAt = DateTimeOffset.UtcNow;
        }

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
        var sorted = actors.OrderBy(a => a.order).ToList();
        short cust = 1;
        foreach (var a in actors)
        {
            _db.MovieActors.Add(new MovieActor
            {
                MovieId = movieId,
                ActorId = a.person.Id,
                CustOrder = cust++,
                CharacterName = a.character
            });
        }
    }

    public async Task ReplaceMovieDirectorsAsync(int movieId, IReadOnlyList<(Person person, short order)> directors, CancellationToken ct)
    {
        var old = await _db.MovieDirectors.Where(x => x.MovieId == movieId).ToListAsync(ct);
        _db.MovieDirectors.RemoveRange(old);

        foreach (var d in directors)
        {
            _db.MovieDirectors.Add(new MovieDirector
            {
                MovieId = movieId,
                DirectorId = d.person.Id,
                BillingOrder = d.order
            });
        }
    }
}
