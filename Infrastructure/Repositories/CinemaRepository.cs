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

    public async Task<List<Cinema>> GetAllAsync(
        string? city = null,
        string? search = null,
        string? sort = null)
    {
        IQueryable<Cinema> q = _db.Cinemas.AsNoTracking();

        // 🔹 Filter by city (normalized)
        if (!string.IsNullOrWhiteSpace(city))
        {
            city = city.Trim();
            q = q.Where(c => c.City != null && c.City.Trim() == city);
        }

        // 🔹 Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            q = q.Where(c =>
                EF.Functions.Like(c.Name, $"%{search}%") ||
                EF.Functions.Like(c.Address, $"%{search}%") ||
                (c.City != null && EF.Functions.Like(c.City, $"%{search}%")));
        }

        // 🔹 Sorting
        q = sort switch
        {
            "name_desc" => q.OrderByDescending(c => c.Name),
            "city" => q.OrderBy(c => c.City ?? "").ThenBy(c => c.Name),
            "city_desc" => q.OrderByDescending(c => c.City ?? "").ThenBy(c => c.Name),
            _ => q.OrderBy(c => c.Name)
        };

        return await q.ToListAsync();
    }

    public Task<Cinema?> GetByIdAsync(int id)
        => _db.Cinemas.FirstOrDefaultAsync(c => c.Id == id);

    public async Task AddAsync(Cinema cinema)
    {
        // Optional normalization on save
        cinema.Name = cinema.Name.Trim();
        cinema.Address = cinema.Address.Trim();
        cinema.City = cinema.City?.Trim();

        _db.Cinemas.Add(cinema);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Cinema cinema)
    {
        cinema.Name = cinema.Name.Trim();
        cinema.Address = cinema.Address.Trim();
        cinema.City = cinema.City?.Trim();

        _db.Cinemas.Update(cinema);
        await _db.SaveChangesAsync();
    }

    // 🔹 Soft delete
    public async Task DeleteAsync(Cinema cinema)
    {
        cinema.IsDeleted = true;
        _db.Cinemas.Update(cinema);
        await _db.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(int id)
        => _db.Cinemas.AnyAsync(c => c.Id == id);

    public Task<bool> HasHallsAsync(int cinemaId)
        => _db.Halls.AnyAsync(h => h.CinemaId == cinemaId);

    public Task<bool> HasSessionsAsync(int cinemaId)
        => _db.Sessions.AnyAsync(s =>
            _db.Halls.Any(h => h.Id == s.HallId && h.CinemaId == cinemaId));

    // 🔹 Normalized list of cities for filter
    public async Task<List<string>> GetCitiesAsync()
    {
        return await _db.Cinemas
            .AsNoTracking()
            .Where(c => c.City != null && c.City.Trim() != "")
            .Select(c => c.City!.Trim())
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();
    }
}
