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
            .Include(m => m.MovieDirectors)
                .ThenInclude(md => md.DirectorId)
            .Include(m => m.MovieCountries)
                .ThenInclude(mc => mc.Country)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }
    public Task<Movie?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
    _db.Movies
        .AsNoTracking()
        .Where(m => m.Id == id && !m.IsDeleted)
        .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
        .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
        .Include(m => m.MovieDirectors).ThenInclude(md => md.Director)
        .Include(m => m.MovieCountries).ThenInclude(mc => mc.Country)
        .Include(m => m.Sessions)
            .ThenInclude(s => s.Hall)
                .ThenInclude(h => h.Cinema)
        .FirstOrDefaultAsync(ct);
}
