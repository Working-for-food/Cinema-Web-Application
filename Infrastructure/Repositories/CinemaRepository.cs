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

    public Task<List<Cinema>> GetAllAsync()
        => _db.Cinemas
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

    public Task<Cinema?> GetByIdAsync(int id)
        => _db.Cinemas
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task AddAsync(Cinema cinema)
    {
        _db.Cinemas.Add(cinema);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Cinema cinema)
    {
        _db.Cinemas.Update(cinema);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Cinema cinema)
    {
        _db.Cinemas.Remove(cinema);
        await _db.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(int id)
        => _db.Cinemas.AnyAsync(c => c.Id == id);
}
