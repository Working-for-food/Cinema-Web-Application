using Application.DTOs;

namespace Application.Interfaces;
public interface ITmdbClient
{
    Task<IReadOnlyList<TmdbMovieSearchItem>> SearchMovieAsync(string query, int page = 1, CancellationToken ct = default);

    Task<TmdbMovieDetailsResponse> GetMovieDetailsAsync(int tmdbMovieId, CancellationToken ct = default);

    Task<TmdbCreditsResponse> GetCreditsAsync(int tmdbMovieId, CancellationToken ct = default);

    Task<TmdbVideosResponse> GetVideosAsync(int tmdbMovieId, CancellationToken ct = default);
}

