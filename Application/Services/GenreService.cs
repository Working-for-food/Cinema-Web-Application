using Application.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Repositories;

namespace Application.Services;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _repo;

    public GenreService(IGenreRepository repo) => _repo = repo;

    public Task<List<Genre>> GetAllAsync(CancellationToken ct = default) =>
        _repo.GetAllAsync(ct);

    public Task<Genre?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _repo.GetByIdAsync(id, ct);

    public async Task<(bool ok, string? error)> CreateAsync(string name, CancellationToken ct = default)
    {
        name = (name ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Name is required.");
        if (name.Length < 2 || name.Length > 60)
            return (false, "Name must be between 2 and 60 characters.");

        var existing = await _repo.GetByNameAsync(name, ct);
        if (existing != null)
            return (false, "Genre with the same name already exists.");

        await _repo.AddAsync(new Genre { Name = name }, ct);
        await _repo.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> UpdateAsync(int id, string name, CancellationToken ct = default)
    {
        name = (name ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Name is required.");
        if (name.Length < 2 || name.Length > 60)
            return (false, "Name must be between 2 and 60 characters.");

        var genre = await _repo.GetByIdAsync(id, ct);
        if (genre == null)
            return (false, "Genre not found.");

        var existing = await _repo.GetByNameAsync(name, ct);
        if (existing != null && existing.Id != id)
            return (false, "Genre with the same name already exists.");

        genre.Name = name;
        await _repo.UpdateAsync(genre, ct);
        await _repo.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        var genre = await _repo.GetByIdAsync(id, ct);
        if (genre == null)
            return (false, "Genre not found.");

        var used = await _repo.AnyMovieUsesGenreAsync(id, ct);
        if (used)
            return (false, "Cannot delete: this genre is used by movies.");

        await _repo.DeleteAsync(genre, ct);
        await _repo.SaveChangesAsync(ct);
        return (true, null);
    }
}
