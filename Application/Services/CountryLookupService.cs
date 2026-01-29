using Application.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Application.Services;

public class CountryLookupService : ICountryLookupService
{
    private readonly ICountryRepository _repo;
    public CountryLookupService(ICountryRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);

    public Task<Country?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _repo.GetByCodeAsync(code, ct);

    public Task<bool> ExistsAsync(string code, CancellationToken ct = default)
        => _repo.ExistsAsync(code, ct);

    public Task<IReadOnlyList<Country>> SearchAsync(string? query, int limit = 20, CancellationToken ct = default)
        => _repo.SearchAsync(query, limit, ct);
}
