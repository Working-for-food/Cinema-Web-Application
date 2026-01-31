using Application.DTOs.Movies;

namespace Application.Interfaces;

public interface IMoviePublicService
{
    Task<MovieDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default);
}
