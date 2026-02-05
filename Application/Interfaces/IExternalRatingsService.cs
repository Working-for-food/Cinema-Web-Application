using Application.DTOs.OmdbDtos;

namespace Application.Interfaces;

public interface IExternalRatingsService
{
    Task<ExternalRatingsDto> GetRatingsAsync(string imdbId, CancellationToken ct = default);
}
