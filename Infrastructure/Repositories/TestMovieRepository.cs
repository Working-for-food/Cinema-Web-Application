using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TestMovieRepository : ITestMovieRepository
{
    private readonly CinemaDbContext _db;
    public TestMovieRepository(CinemaDbContext db) => _db = db;

    public Task<List<Movie>> SearchAsync(string? query, int take, CancellationToken ct)
    {
        var q = _db.Movies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(m => m.Title.Contains(query));

        return q.OrderBy(m => m.Title)
                .Take(take)
                .ToListAsync(ct);
    }
}
