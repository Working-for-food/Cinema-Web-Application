using Application.DTOs;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;

namespace Application.Services;

public class MovieService
{
    private const int DefaultPageSize = 10;
    private static readonly DateOnly MinReleaseDate = new(1888, 1, 1); // first films era

    private readonly IMovieRepository _movies;
    private readonly IGenreRepository _genres;

    public MovieService(IMovieRepository movies, IGenreRepository genres)
    {
        _movies = movies;
        _genres = genres;
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

    public async Task<MovieFormDto> GetMovieForEditAsync(int id, CancellationToken ct = default)
    {
        var movie = await _movies.GetByIdAsync(id, ct);
        if (movie == null)
            throw new InvalidOperationException("Movie not found.");

        return new MovieFormDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Duration = movie.Duration,
            ReleaseDate = movie.ReleaseDate,
            ProductionCountryCode = movie.ProductionCountryCode,
            GenreIds = movie.MovieGenres.Select(mg => mg.GenreId).ToList()
        };
    }

    public async Task<Movie?> GetMovieDetailsAsync(int id, CancellationToken ct = default)
    {
        return await _movies.GetByIdWithDetailsAsync(id, ct);
    }

    public async Task CreateMovieAsync(MovieFormDto dto, CancellationToken ct = default)
    {
        await ValidateMovieAsync(dto, ct);

        var movie = new Movie
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            ReleaseDate = dto.ReleaseDate,
            Duration = dto.Duration,
            ProductionCountryCode = NormalizeCountryCode(dto.ProductionCountryCode)
        };

        await _movies.AddAsync(movie, DistinctIds(dto.GenreIds), ct);
    }

    public async Task UpdateMovieAsync(MovieFormDto dto, CancellationToken ct = default)
    {
        if (dto.Id <= 0)
            throw new InvalidOperationException("Invalid movie id.");

        await ValidateMovieAsync(dto, ct);

        var movie = new Movie
        {
            Id = dto.Id,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            ReleaseDate = dto.ReleaseDate,
            Duration = dto.Duration,
            ProductionCountryCode = NormalizeCountryCode(dto.ProductionCountryCode)
        };

        await _movies.UpdateAsync(movie, DistinctIds(dto.GenreIds), ct);
    }

    public async Task DeleteMovieAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0)
            throw new InvalidOperationException("Invalid movie id.");

        var usedInSessions = await _movies.AnySessionsAsync(id, ct);
        if (usedInSessions)
            throw new InvalidOperationException("Неможливо видалити фільм: для нього вже створені сеанси.");

        await _movies.DeleteAsync(id, ct);
    }

    public Task<List<Genre>> GetGenresAsync(CancellationToken ct = default) =>
        _genres.GetAllAsync(ct);

    private async Task ValidateMovieAsync(MovieFormDto dto, CancellationToken ct)
    {
        // title
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new InvalidOperationException("Title is required.");
        if (dto.Title.Trim().Length > 200)
            throw new InvalidOperationException("Title must be <= 200 characters.");

        // description
        if (dto.Description != null && dto.Description.Length > 4000)
            throw new InvalidOperationException("Description is too long (max 4000 characters).");

        // duration
        if (dto.Duration.HasValue)
        {
            if (dto.Duration.Value <= 0)
                throw new InvalidOperationException("Duration must be positive.");
            if (dto.Duration.Value > 600)
                throw new InvalidOperationException("Duration must be <= 600 min.");
        }

        // release date
        if (dto.ReleaseDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dto.ReleaseDate.Value < MinReleaseDate)
                throw new InvalidOperationException($"Release date must be after {MinReleaseDate:yyyy-MM-dd}.");
            if (dto.ReleaseDate.Value > today.AddYears(2))
                throw new InvalidOperationException("Release date looks too far in the future.");
        }

        // country code
        if (!string.IsNullOrWhiteSpace(dto.ProductionCountryCode))
        {
            var code = dto.ProductionCountryCode.Trim();
            if (code.Length != 2 || !code.All(char.IsLetter))
                throw new InvalidOperationException("Production country code must be 2 letters (e.g. UA, US).");
        }

        // genres
        var ids = DistinctIds(dto.GenreIds);
        if (ids.Count == 0)
            throw new InvalidOperationException("Select at least one genre.");

        var existing = await _genres.GetAllAsync(ct);
        var existingIds = existing.Select(g => g.Id).ToHashSet();
        var invalid = ids.Where(x => !existingIds.Contains(x)).ToList();
        if (invalid.Count > 0)
            throw new InvalidOperationException("Some selected genres do not exist.");
    }

    private static string? NormalizeCountryCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        return code.Trim().ToUpperInvariant();
    }

    private static List<int> DistinctIds(IEnumerable<int>? ids)
    {
        if (ids == null) return new List<int>();
        return ids.Where(x => x > 0).Distinct().ToList();
    }
}
