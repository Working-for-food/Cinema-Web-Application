using Application.DTOs;
using Infrastructure.Entities;

namespace Application.Interfaces;

public interface IMovieService
{
    Task<(IEnumerable<MovieDto> List, int TotalCount)> GetMoviesAsync(
        string? search,
        string? sortBy,
        int page,
        CancellationToken ct = default);

    Task<Movie?> GetMovieDetailsAsync(int id, CancellationToken ct = default);

    Task<MovieFormDto?> GetMovieForEditAsync(int id, CancellationToken ct = default);

    Task<(bool ok, string? error)> CreateAsync(MovieFormDto dto, CancellationToken ct = default);
    Task<(bool ok, string? error)> UpdateAsync(MovieFormDto dto, CancellationToken ct = default);
    Task<(bool ok, string? error)> DeleteAsync(int id, CancellationToken ct = default);

    Task<List<Genre>> GetGenresAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Country>> GetCountriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Person>> GetDirectorsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Person>> GetActorsAsync(CancellationToken ct = default);
}
