using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class HallRepository : IHallRepository
{
    private readonly CinemaDbContext _db;

    public HallRepository(CinemaDbContext db) => _db = db;

    public Task<List<Hall>> GetAllWithCinemaAsync()
        => _db.Halls
            .Include(h => h.Cinema)
            .AsNoTracking()
            .OrderBy(h => h.Cinema.Name).ThenBy(h => h.Name)
            .ToListAsync();

    public Task<List<Hall>> GetByCinemaWithCinemaAsync(int cinemaId)
        => _db.Halls
            .Where(h => h.CinemaId == cinemaId)
            .Include(h => h.Cinema)
            .AsNoTracking()
            .OrderBy(h => h.Name)
            .ToListAsync();

    public Task<Hall?> GetByIdAsync(int id)
        => _db.Halls.FirstOrDefaultAsync(h => h.Id == id);

    public Task<Hall?> GetByIdWithCinemaAsync(int id)
        => _db.Halls
            .Include(h => h.Cinema)
            .FirstOrDefaultAsync(h => h.Id == id);

    public async Task AddAsync(Hall hall)
    {
        _db.Halls.Add(hall);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Hall hall)
    {
        _db.Halls.Update(hall);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Hall hall)
    {
        _db.Halls.Remove(hall);
        await _db.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(int hallId)
        => _db.Halls.AnyAsync(h => h.Id == hallId);

    public Task<bool> HasAnySessionsAsync(int hallId)
        => _db.Sessions.AnyAsync(s => s.HallId == hallId);

    public Task<bool> HasAnyBookingsAsync(int hallId)
        => _db.Bookings
            .Join(_db.Sessions,
                b => b.SessionId,
                s => s.Id,
                (b, s) => new { b, s })
            .AnyAsync(x => x.s.HallId == hallId);
}
