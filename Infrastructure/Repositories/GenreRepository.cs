using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GenreRepository : IGenreRepository
{
    private readonly CinemaDbContext _db;

    public GenreRepository(CinemaDbContext db) => _db = db;

    public Task<List<Genre>> GetAllAsync(CancellationToken ct = default) =>
        _db.Genres
           .AsNoTracking()
           .OrderBy(g => g.Name)
           .ToListAsync(ct);

    public Task<Genre?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Genres.FirstOrDefaultAsync(g => g.Id == id, ct);

    public Task<Genre?> GetByNameAsync(string name, CancellationToken ct = default) =>
        _db.Genres.FirstOrDefaultAsync(g => g.Name == name, ct);

    public Task AddAsync(Genre genre, CancellationToken ct = default) =>
        _db.Genres.AddAsync(genre, ct).AsTask();

    public Task UpdateAsync(Genre genre, CancellationToken ct = default)
    {
        _db.Genres.Update(genre);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Genre genre, CancellationToken ct = default)
    {
        _db.Genres.Remove(genre);
        return Task.CompletedTask;
    }

    public Task<bool> AnyMovieUsesGenreAsync(int genreId, CancellationToken ct = default) =>
        _db.MovieGenres.AnyAsync(mg => mg.GenreId == genreId, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
