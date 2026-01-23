using Infrastructure.Entities;

namespace Infrastructure.Repositories;

public interface IGenreRepository
{
    Task<List<Genre>> GetAllAsync(CancellationToken ct = default);
    Task<Genre?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Genre?> GetByNameAsync(string name, CancellationToken ct = default);

    Task AddAsync(Genre genre, CancellationToken ct = default);
    Task UpdateAsync(Genre genre, CancellationToken ct = default);
    Task DeleteAsync(Genre genre, CancellationToken ct = default);
    Task<bool> AnyMovieUsesGenreAsync(int genreId, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
