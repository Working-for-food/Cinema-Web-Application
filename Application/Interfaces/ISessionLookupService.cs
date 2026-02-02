using Application.DTOs;

namespace Application.Interfaces;

public interface ISessionLookupService
{
    Task<List<LookupItemDto>> GetMoviesAsync(string? query, CancellationToken ct);
    Task<List<LookupItemDto>> GetHallsAsync(CancellationToken ct);
    Task<List<LookupItemDto>> GetCinemasAsync(CancellationToken ct);
    Task<List<LookupItemDto>> GetHallsByCinemaAsync(int cinemaId, CancellationToken ct);
    Task<string?> GetMovieTitleByIdAsync(int movieId, CancellationToken ct);
}
