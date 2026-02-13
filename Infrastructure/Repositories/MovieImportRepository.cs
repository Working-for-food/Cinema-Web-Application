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

        var (first, middle, last) = SplitFullNameParts(fullName);

        // Existing TMDB person
        var person = await _db.People.FirstOrDefaultAsync(p => p.TmdbId == tmdbPersonId, ct);
        if (person != null)
        {
            person.FullName = fullName;

            
            if (string.IsNullOrWhiteSpace(person.FirstName) && !string.IsNullOrWhiteSpace(first))
                person.FirstName = first;

            if (string.IsNullOrWhiteSpace(person.MiddleName) && !string.IsNullOrWhiteSpace(middle))
                person.MiddleName = middle;

            if (string.IsNullOrWhiteSpace(person.LastName) && !string.IsNullOrWhiteSpace(last))
                person.LastName = last;

            if (!string.IsNullOrWhiteSpace(photoPath))
                person.PhotoUrl = photoPath;

            person.TmdbLastSyncAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
            return person;
        }

        // Manual person with same FullName (link to TMDB)
        var manual = await _db.People.FirstOrDefaultAsync(p => p.TmdbId == null && p.FullName == fullName, ct);
        if (manual != null)
        {
            manual.TmdbId = tmdbPersonId;

            if (string.IsNullOrWhiteSpace(manual.FirstName) && !string.IsNullOrWhiteSpace(first))
                manual.FirstName = first;

            if (string.IsNullOrWhiteSpace(manual.MiddleName) && !string.IsNullOrWhiteSpace(middle))
                manual.MiddleName = middle;

            if (string.IsNullOrWhiteSpace(manual.LastName) && !string.IsNullOrWhiteSpace(last))
                manual.LastName = last;

            if (!string.IsNullOrWhiteSpace(photoPath))
                manual.PhotoUrl = photoPath;

            manual.TmdbLastSyncAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
            return manual;
        }

        // New person
        person = new Person
        {
            TmdbId = tmdbPersonId,
            FullName = fullName,
            FirstName = first,
            MiddleName = middle,
            LastName = last,
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
        foreach (var a in unique)
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
        var unique = directors
            .Where(d => d.person != null)
            .GroupBy(d => d.person.Id)
            .Select(g => g.OrderBy(x => x.order).First())
            .OrderBy(x => x.order)
            .ToList();

        short billing = 1;
        foreach (var d in unique)
        {
            _db.MovieDirectors.Add(new MovieDirector
            {
                MovieId = movieId,
                DirectorId = d.person.Id,
                BillingOrder = billing++
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

    private static readonly HashSet<string> Honorifics = new(StringComparer.OrdinalIgnoreCase)
{
    "mr", "mr.", "mrs", "mrs.", "ms", "ms.", "miss",
    "dr", "dr.", "prof", "prof.", "sir", "dame"
};

    private static readonly HashSet<string> Suffixes = new(StringComparer.OrdinalIgnoreCase)
{
    "jr", "jr.", "sr", "sr.",
    "ii", "iii", "iv", "v", "vi", "vii", "viii", "ix", "x"
};

    private static readonly HashSet<string> LastNameParticles = new(StringComparer.OrdinalIgnoreCase)
{
    
    "da", "de", "del", "della", "di", "du",
    "la", "le", "lo",
    "van", "von", "der", "den", "ten", "ter",
    "al", "el",
    "bin", "ibn",
    "dos", "das", "do", "de la", "de los" 
};

    private static (string? First, string? Middle, string? Last) SplitFullNameParts(string fullName)
    {
        var name = NormalizeWs(fullName);
        if (string.IsNullOrWhiteSpace(name))
            return (null, null, null);

        
        if (name.Contains(','))
        {
            var parts = name.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var lastPart = parts.Length > 0 ? NormalizeWs(parts[0]) : "";
            var rest = parts.Length > 1 ? NormalizeWs(string.Join(' ', parts.Skip(1))) : "";

            var (firstR, middleR, lastR) = SplitByTokens(rest);
            var last = string.IsNullOrWhiteSpace(lastPart) ? lastR : lastPart;

            return (Trim60(firstR), Trim60(middleR), Trim60(last));
        }

        return SplitByTokens(name);
    }

    private static (string? First, string? Middle, string? Last) SplitByTokens(string name)
    {
        var tokens = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        if (tokens.Count == 0) return (null, null, null);

        
        while (tokens.Count > 1 && Honorifics.Contains(tokens[0].Trim()))
            tokens.RemoveAt(0);

        if (tokens.Count == 1)
            return (Trim60(tokens[0]), null, null);

        
        string? suffix = null;
        if (tokens.Count >= 2 && Suffixes.Contains(tokens[^1].Trim()))
        {
            suffix = tokens[^1].Trim();
            tokens.RemoveAt(tokens.Count - 1);
        }

        if (tokens.Count == 1)
            return (Trim60(tokens[0]), null, Trim60(suffix)); 

        
        int lastStart = tokens.Count - 1;
        while (lastStart - 1 >= 1 && LastNameParticles.Contains(tokens[lastStart - 1].Trim().ToLowerInvariant()))
            lastStart--;

        var first = tokens[0];
        var middleTokens = tokens.Skip(1).Take(lastStart - 1).ToList();
        var lastTokens = tokens.Skip(lastStart).ToList();

        var middle = middleTokens.Count > 0 ? string.Join(' ', middleTokens) : null;
        var last = lastTokens.Count > 0 ? string.Join(' ', lastTokens) : null;

        if (!string.IsNullOrWhiteSpace(suffix))
            last = string.IsNullOrWhiteSpace(last) ? suffix : $"{last} {suffix}";

        return (Trim60(first), Trim60(middle), Trim60(last));
    }

    private static string? Trim60(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        s = NormalizeWs(s);
        return s.Length <= 60 ? s : s[..60];
    }

}
