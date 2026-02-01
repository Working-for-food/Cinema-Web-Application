using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Application.Services;

public class MovieService : IMovieService
{
    private const int DefaultPageSize = 10;
    private static readonly DateOnly MinReleaseDate = new(1888, 1, 1);

    private readonly IMovieRepository _movies;
    private readonly IGenreRepository _genres;
    private readonly ICountryRepository _countries;
    private readonly IPersonRepository _people;

    public MovieService(
        IMovieRepository movies,
        IGenreRepository genres,
        ICountryRepository countries,
        IPersonRepository people)
    {
        _movies = movies;
        _genres = genres;
        _countries = countries;
        _people = people;
    }

    public async Task<(IEnumerable<MovieDto> List, int TotalCount)> GetMoviesAsync(
        string? search,
        string? sortBy,
        int page,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;

        var result = await _movies.GetAllAsync(search, sortBy, page, DefaultPageSize, ct);

        var dtos = result.Items.Select(m => new MovieDto
        {
            Id = m.Id,
            Title = m.Title,
            ReleaseDate = m.ReleaseDate,
            Duration = m.Duration,
            GenreNames = string.Join(", ", m.MovieGenres.Select(mg => mg.Genre.Name)),
            SessionsCount = m.Sessions?.Count ?? 0
        });

        return (dtos, result.TotalCount);
    }

    public Task<Movie?> GetMovieDetailsAsync(int id, CancellationToken ct = default) =>
        _movies.GetByIdWithDetailsAsync(id, ct);

    public async Task<MovieFormDto?> GetMovieForEditAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return null;

        var movie = await _movies.GetByIdAsync(id, ct);
        if (movie == null) return null;

        return new MovieFormDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Duration = movie.Duration,
            ReleaseDate = movie.ReleaseDate,
            GenreIds = movie.MovieGenres.Select(mg => mg.GenreId).ToList(),
            ActorIds = movie.MovieActors.Select(ma => ma.ActorId).ToList(),
            CountryCodes = movie.MovieCountries.Select(mc => mc.CountryCode).ToList(),
            DirectorIds = movie.MovieDirectors.Select(md => md.DirectorId).ToList()
        };
    }

    public async Task<(bool ok, string? error)> CreateAsync(MovieFormDto dto, CancellationToken ct = default)
    {
        var err = await ValidateMovieAsync(dto, ct);
        if (err != null) return (false, err);

        var movie = new Movie
        {
            Title = dto.Title.Trim(),
            Description = NormalizeNullable(dto.Description),
            ReleaseDate = dto.ReleaseDate,
            Duration = dto.Duration
        };

        await _movies.AddAsync(movie, DistinctIds(dto.GenreIds), DistinctIds(dto.ActorIds), DistinctCodesStrict(dto.CountryCodes, out _), DistinctIds(dto.DirectorIds), ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> UpdateAsync(MovieFormDto dto, CancellationToken ct = default)
    {
        if (dto.Id <= 0) return (false, "Invalid movie id.");

        var err = await ValidateMovieAsync(dto, ct);
        if (err != null) return (false, err);

        var movie = new Movie
        {
            Id = dto.Id,
            Title = dto.Title.Trim(),
            Description = NormalizeNullable(dto.Description),
            ReleaseDate = dto.ReleaseDate,
            Duration = dto.Duration
        };

        await _movies.UpdateAsync(movie, DistinctIds(dto.GenreIds), DistinctIds(dto.ActorIds), DistinctCodesStrict(dto.CountryCodes, out _), DistinctIds(dto.DirectorIds), ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return (false, "Invalid movie id.");

        var usedInSessions = await _movies.AnySessionsAsync(id, ct);
        if (usedInSessions)
            return (false, "Неможливо видалити фільм: для нього вже створені сеанси.");

        await _movies.DeleteAsync(id, ct);
        return (true, null);
    }

    public Task<List<Genre>> GetGenresAsync(CancellationToken ct = default) =>
        _genres.GetAllAsync(ct);

    public Task<IReadOnlyList<Country>> GetCountriesAsync(CancellationToken ct = default) =>
        _countries.GetAllAsync(ct);

    public Task<IReadOnlyList<Person>> GetDirectorsAsync(CancellationToken ct = default) =>
    _people.GetDirectorsAsync(ct);

    public async Task<IReadOnlyList<Person>> GetActorsAsync(CancellationToken ct = default)
    {
        var (items, _) = await _people.GetAllAsync(null, 1, 500, ct);
        return items;
    }

    // ---------------- validation ----------------

    private async Task<string?> ValidateMovieAsync(MovieFormDto dto, CancellationToken ct)
    {
        dto.Title = (dto.Title ?? "").Trim();
        dto.Description = NormalizeNullable(dto.Description);

        if (string.IsNullOrWhiteSpace(dto.Title))
            return "Title is required.";
        if (dto.Title.Length > 200)
            return "Title must be <= 200 characters.";

        if (dto.Description != null && dto.Description.Length > 4000)
            return "Description is too long (max 4000 characters).";

        if (dto.Duration.HasValue && dto.Duration.Value <= 0)
            return "Duration must be positive.";
        if (dto.Duration.HasValue && dto.Duration.Value > 600)
            return "Duration must be <= 600 min.";

        if (dto.ReleaseDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dto.ReleaseDate.Value < MinReleaseDate)
                return $"Release date must be after {MinReleaseDate:yyyy-MM-dd}.";
            if (dto.ReleaseDate.Value > today.AddYears(2))
                return "Release date looks too far in the future.";
        }

        var genreIds = DistinctIds(dto.GenreIds);
        if (genreIds.Count == 0)
            return "Select at least one genre.";

        
        var existingGenres = await _genres.GetAllAsync(ct);
        var existingGenreIds = existingGenres.Select(g => g.Id).ToHashSet();
        if (genreIds.Any(id => !existingGenreIds.Contains(id)))
            return "Some selected genres do not exist.";


        var directorIds = DistinctIds(dto.DirectorIds);
        foreach (var directorId in directorIds)
        {
            var director = await _people.GetByIdAsync(directorId, ct);
            if (director == null)
                return "Some selected directors do not exist.";
        }


        var actorIds = DistinctIds(dto.ActorIds);
        foreach (var actorId in actorIds)
        {
            var actor = await _people.GetByIdAsync(actorId, ct);
            if (actor == null)
                return "Some selected actors do not exist.";
        }

        
        _ = DistinctCodesStrict(dto.CountryCodes, out var invalid);
        if (invalid.Count > 0)
            return "Some selected countries have invalid code format (must be 2 letters).";

        
        var codes = DistinctCodesStrict(dto.CountryCodes, out _);
        if (codes.Count > 0)
        {
            var all = await _countries.GetAllAsync(ct);
            var set = all.Select(x => x.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (codes.Any(c => !set.Contains(c)))
                return "Some selected countries do not exist.";
        }

        return null;
    }

    private static string? NormalizeCountryCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        return code.Trim().ToUpperInvariant();
    }

    private static string? NormalizeNullable(string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static List<int> DistinctIds(IEnumerable<int>? ids) =>
        ids == null ? new List<int>() : ids.Where(x => x > 0).Distinct().ToList();

    private static List<string> DistinctCodesStrict(IEnumerable<string>? codes, out List<string> invalidCodes)
    {
        invalidCodes = new List<string>();
        if (codes == null) return new List<string>();

        var normalized = new List<string>();
        foreach (var raw in codes)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var c = raw.Trim().ToUpperInvariant();

            if (c.Length != 2 || !c.All(char.IsLetter))
            {
                invalidCodes.Add(raw);
                continue;
            }

            normalized.Add(c);
        }

        return normalized.Distinct().ToList();
    }
}
