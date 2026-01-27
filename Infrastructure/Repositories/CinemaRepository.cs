using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CinemaRepository : ICinemaRepository
{
    private readonly CinemaDbContext _db;

    public CinemaRepository(CinemaDbContext db)
    {
        _db = db;
    }

    private IQueryable<Cinema> CinemasQuery(bool asTracking, bool includeDeleted)
    {
        var q = asTracking ? _db.Cinemas : _db.Cinemas.AsNoTracking();
        if (!includeDeleted)
            q = q.Where(c => !c.IsDeleted);
        return q;
    }

    public async Task<List<Cinema>> GetAllAsync(
        string? city = null,
        string? search = null,
        string? sort = null,
        bool includeDeleted = false,
        CancellationToken ct = default)
    {
        IQueryable<Cinema> q = CinemasQuery(asTracking: false, includeDeleted: includeDeleted);

        if (!string.IsNullOrWhiteSpace(city))
        {
            city = city.Trim();
            q = q.Where(c => c.City != null && c.City == city);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            q = q.Where(c =>
                EF.Functions.Like(c.Name, $"%{search}%") ||
                EF.Functions.Like(c.Address, $"%{search}%") ||
                (c.City != null && EF.Functions.Like(c.City, $"%{search}%")));
        }

        q = sort switch
        {
            "name_desc" => q.OrderByDescending(c => c.Name),
            "city" => q.OrderBy(c => c.City ?? "").ThenBy(c => c.Name),
            "city_desc" => q.OrderByDescending(c => c.City ?? "").ThenBy(c => c.Name),
            _ => q.OrderBy(c => c.Name)
        };

        return await q.ToListAsync(ct);
    }

    public Task<Cinema?> GetByIdAsync(
        int id,
        bool asTracking = true,
        bool includeDeleted = false,
        CancellationToken ct = default)
        => CinemasQuery(asTracking, includeDeleted)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(Cinema cinema, CancellationToken ct = default)
    {
        _db.Cinemas.Add(cinema);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Cinema cinema, CancellationToken ct = default)
    {
        _db.Cinemas.Update(cinema);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Cinema cinema, CancellationToken ct = default)
    {
        cinema.IsDeleted = true;
        _db.Cinemas.Update(cinema);
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsAsync(int id, bool includeDeleted = false, CancellationToken ct = default)
        => CinemasQuery(asTracking: false, includeDeleted: includeDeleted)
            .AnyAsync(c => c.Id == id, ct);

    public Task<bool> HasHallsAsync(int cinemaId, CancellationToken ct = default)
        => _db.Halls.AnyAsync(h => h.CinemaId == cinemaId, ct);

    public Task<bool> HasSessionsAsync(int cinemaId, CancellationToken ct = default)
        => _db.Sessions.AnyAsync(s =>
            _db.Halls.Any(h => h.Id == s.HallId && h.CinemaId == cinemaId), ct);

    public async Task<List<string>> GetCitiesAsync(bool includeDeleted = false, CancellationToken ct = default)
    {
        return await CinemasQuery(asTracking: false, includeDeleted: includeDeleted)
            .Where(c => c.City != null && c.City != "")
            .Select(c => c.City!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(ct);
    }
}
