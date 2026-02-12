using Application.DTOs.Movies;

namespace Application.Interfaces;

public interface IMoviePublicService
{

    Task<IReadOnlyList<MovieDetailsDtoRelatedItem>> GetRelatedMoviesAsync(
    int movieId,
    IReadOnlyList<string> genres,
    int take,
    CancellationToken ct = default);

    Task<MovieDetailsDto?> GetDetailsAsync(int movieId, DateOnly? date, CancellationToken ct = default);
}
