using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CountryRepository : ICountryRepository
{
    private readonly CinemaDbContext _db;
    public CountryRepository(CinemaDbContext db) => _db = db;
    public async Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Countries
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }
    public async Task<Country?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        code = (code ?? "").Trim().ToUpperInvariant();
        return await _db.Countries
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == code, ct);
    }

    public async Task<IReadOnlyList<Country>> SearchAsync(string? query, int limit = 20, CancellationToken ct = default)
    {
        query = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
        limit = Math.Clamp(limit, 1, 50);
        IQueryable<Country> q = _db.Countries.AsNoTracking();
        if (query is not null)
        {
            var upper = query.ToUpperInvariant();

            q = q.Where(c =>
                c.Code.StartsWith(upper) ||
                c.Name.Contains(query));
        }
        return await q.OrderBy(c => c.Name)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Country country, CancellationToken ct = default)
    {
        country.Code = country.Code.Trim().ToUpperInvariant();
        country.Name = country.Name.Trim();

        _db.Countries.Add(country);
        await _db.SaveChangesAsync(ct);
    }
    public async Task UpdateAsync(Country country, CancellationToken ct = default)
    {
        country.Code = country.Code.Trim().ToUpperInvariant();
        country.Name = country.Name.Trim();

        _db.Countries.Update(country);
        await _db.SaveChangesAsync(ct);
    }
    public async Task DeleteAsync(Country country, CancellationToken ct = default)
    {
        _db.Countries.Remove(country);
        await _db.SaveChangesAsync(ct);
    }
}
