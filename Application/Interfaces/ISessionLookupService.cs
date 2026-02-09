using Application.DTOs;
using Application.DTOs.Pricing;

namespace Application.Interfaces;

public interface ISessionLookupService
{
    Task<List<LookupItemDto>> GetMoviesAsync(string? query, CancellationToken ct);
    Task<List<LookupItemDto>> GetHallsAsync(CancellationToken ct);
    Task<List<LookupItemDto>> GetCinemasAsync(CancellationToken ct);
    Task<List<LookupItemDto>> GetHallsByCinemaAsync(int cinemaId, CancellationToken ct);
    Task<List<SeatDto>> GetHallSeatsAsync(int hallId, CancellationToken ct);
    Task<string?> GetMovieTitleByIdAsync(int movieId, CancellationToken ct);
    Task<HallPricingMetaDto> GetHallPricingMetaAsync(int hallId, CancellationToken ct);
}
