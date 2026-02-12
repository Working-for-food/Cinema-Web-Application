using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface IUserMovieRepository
{
    Task<Movie?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Movie>> GetRelatedMoviesAsync(
    int movieId,
    IReadOnlyList<string> genres,
    int take,
    DateTime nowUtc,
    CancellationToken ct = default);

    Task<Movie?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
}
