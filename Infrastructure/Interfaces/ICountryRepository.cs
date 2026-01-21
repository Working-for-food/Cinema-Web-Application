using Infrastructure.Entities;

namespace Infrastructure.Interfaces;
public interface ICountryRepository
{
    Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct = default);

    Task<Country?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Country>> SearchAsync(
        string? query,
        int limit = 20,
        CancellationToken ct = default
    );
    Task AddAsync(Country country, CancellationToken ct = default);
    Task UpdateAsync(Country country, CancellationToken ct = default);
    Task DeleteAsync(Country country, CancellationToken ct = default);
}

