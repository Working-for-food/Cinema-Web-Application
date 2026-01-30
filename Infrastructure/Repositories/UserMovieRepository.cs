using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserMovieRepository : IUserMovieRepository
{
    private readonly CinemaDbContext _db;

    public UserMovieRepository(CinemaDbContext db)
    {
        _db = db;
    }

    public Task<Movie?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return _db.Movies
            .AsNoTracking()
            .Include(m => m.Director)
            .Include(m => m.ProductionCountry)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }
}
