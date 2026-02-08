using Application.DTOs.Movies;

namespace Application.Interfaces;

public interface IMoviePublicService
{
    Task<MovieDetailsDto?> GetDetailsAsync(int movieId, DateOnly? date, CancellationToken ct = default);
}
