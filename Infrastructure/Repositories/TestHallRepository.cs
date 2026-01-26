using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TestHallRepository : ITestHallRepository
{
    private readonly CinemaDbContext _db;
    public TestHallRepository(CinemaDbContext db) => _db = db;

    public Task<List<Hall>> GetAllWithCinemaAsync(CancellationToken ct)
        => _db.Halls.AsNoTracking()
            .Include(h => h.Cinema)
            .OrderBy(h => h.Cinema.Name).ThenBy(h => h.Name)
            .ToListAsync(ct);
}
