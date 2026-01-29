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
        var norm = NormalizeCode(code);
        if (norm == null) return null;

        return await _db.Countries
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == norm, ct);
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken ct = default)
    {
        var norm = NormalizeCode(code);
        if (norm == null) return false;

        return await _db.Countries
            .AsNoTracking()
            .AnyAsync(c => c.Code == norm, ct);
    }

    public async Task<IReadOnlyList<Country>> SearchAsync(string? query, int limit = 20, CancellationToken ct = default)
    {
        query = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
        limit = Math.Clamp(limit, 1, 50);

        IQueryable<Country> q = _db.Countries.AsNoTracking();

        if (query is not null)
        {
            var upper = query.ToUpperInvariant();
            q = q.Where(c => c.Code.StartsWith(upper) || c.Name.Contains(query));
        }

        return await q.OrderBy(c => c.Name)
            .Take(limit)
            .ToListAsync(ct);
    }

    private static string? NormalizeCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var x = code.Trim().ToUpperInvariant();
        if (x.Length != 2) return null;
        if (!x.All(char.IsLetter)) return null;
        return x;
    }
}
