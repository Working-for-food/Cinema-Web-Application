using Infrastructure.Entities;

namespace Application.Interfaces;

public interface ICountryLookupService
{
    Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct = default);
    Task<Country?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> ExistsAsync(string code, CancellationToken ct = default);

    Task<IReadOnlyList<Country>> SearchAsync(string? query, int limit = 20, CancellationToken ct = default);
}
