using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface IPersonRepository
{
    Task<(IReadOnlyList<Person> Items, int TotalCount)> GetAllAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<Person?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Person person, CancellationToken ct = default);
    Task UpdateAsync(Person person, CancellationToken ct = default);
    Task DeleteAsync(Person person, CancellationToken ct = default);

    Task<bool> IsUsedAsync(int personId, CancellationToken ct = default);
}
