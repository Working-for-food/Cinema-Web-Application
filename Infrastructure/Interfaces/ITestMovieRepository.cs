using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface ITestMovieRepository
{
    Task<List<Movie>> SearchAsync(string? query, int take, CancellationToken ct);
    Task<string?> GetTitleByIdAsync(int id, CancellationToken ct);
}