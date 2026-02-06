using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly CinemaDbContext _db;
    public PersonRepository(CinemaDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Person> Items, int TotalCount)> GetAllAsync(
        string? search, int page, int pageSize, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        IQueryable<Person> q = _db.People.AsNoTracking().Include(p => p.Country);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(p =>
                p.FirstName.Contains(term) ||
                (p.MiddleName != null && p.MiddleName.Contains(term)) ||
                p.LastName.Contains(term) ||
                (p.FullName != null && p.FullName.Contains(term)));
        }

        q = q.OrderBy(p => p.LastName).ThenBy(p => p.FirstName);

        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, total);
    }

    public Task<Person?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.People
            .Include(p => p.Country)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(Person person, CancellationToken ct = default)
    {
        _db.People.Add(person);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Person person, CancellationToken ct = default)
    {
        _db.People.Update(person);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Person person, CancellationToken ct = default)
    {
        _db.People.Remove(person);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> IsUsedAsync(int personId, CancellationToken ct = default)
    {
        var usedAsActor = await _db.MovieActors.AsNoTracking().AnyAsync(x => x.ActorId == personId, ct);
        if (usedAsActor) return true;

        var usedAsDirector = await _db.MovieDirectors.AsNoTracking().AnyAsync(x => x.DirectorId == personId, ct);
        return usedAsDirector;
    }
    public async Task<IReadOnlyList<Person>> GetDirectorsAsync(CancellationToken ct = default)
    {
        return await _db.People
            .AsNoTracking()
            .OrderBy(p => p.FullName)
            .ToListAsync(ct);
    }

    public Task<Person?> GetByFullNameAsync(string fullName, CancellationToken ct = default) =>
    _db.People.AsNoTracking().FirstOrDefaultAsync(p => p.FullName == fullName, ct);

}
