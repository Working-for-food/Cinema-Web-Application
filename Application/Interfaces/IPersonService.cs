using Infrastructure.Entities;

namespace Application.Interfaces;

public interface IPersonService
{
    Task<(IReadOnlyList<Person> Items, int TotalCount)> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<Person?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<(bool ok, string? error)> CreateAsync(Person person, CancellationToken ct = default);
    Task<(bool ok, string? error)> UpdateAsync(Person person, CancellationToken ct = default);
    Task<(bool ok, string? error)> DeleteAsync(int id, CancellationToken ct = default);
}
