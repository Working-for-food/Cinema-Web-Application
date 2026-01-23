using Infrastructure.Entities;

namespace Application.Interfaces;

public interface IGenreService
{
    Task<List<Genre>> GetAllAsync(CancellationToken ct = default);
    Task<Genre?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<(bool ok, string? error)> CreateAsync(string name, CancellationToken ct = default);
    Task<(bool ok, string? error)> UpdateAsync(int id, string name, CancellationToken ct = default);
    Task<(bool ok, string? error)> DeleteAsync(int id, CancellationToken ct = default);
}
