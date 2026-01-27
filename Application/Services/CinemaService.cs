using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Application.Services;

public class CinemaService : ICinemaService
{
    private readonly ICinemaRepository _repo;

    public CinemaService(ICinemaRepository repo)
    {
        _repo = repo;
    }

    private static string NormalizeRequired(string value)
        => (value ?? "").Trim();

    public async Task<List<CinemaListDto>> GetAllAsync(string? city = null, string? search = null, string? sort = null, CancellationToken ct = default)
    {
        city = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        var cinemas = await _repo.GetAllAsync(city, search, sort, includeDeleted: false, ct);

        return cinemas.Select(c => new CinemaListDto
        {
            Id = c.Id,
            Name = c.Name,
            Address = c.Address,
            City = c.City ?? ""
        }).ToList();
    }

    public Task<List<string>> GetCitiesAsync(CancellationToken ct = default)
        => _repo.GetCitiesAsync(includeDeleted: false, ct);

    public async Task<CinemaEditDto?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var cinema = await _repo.GetByIdAsync(id, asTracking: false, includeDeleted: false, ct);
        if (cinema == null) return null;

        return new CinemaEditDto
        {
            Id = cinema.Id,
            Name = cinema.Name,
            Address = cinema.Address,
            City = cinema.City ?? ""
        };
    }

    public async Task<CinemaDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        var cinema = await _repo.GetByIdAsync(id, asTracking: false, includeDeleted: false, ct);
        if (cinema == null) return null;

        return new CinemaDetailsDto
        {
            Id = cinema.Id,
            Name = cinema.Name,
            Address = cinema.Address,
            City = cinema.City ?? ""
        };
    }

    public async Task<int> CreateAsync(CinemaEditDto dto, CancellationToken ct = default)
    {
        var cinema = new Cinema
        {
            Name = NormalizeRequired(dto.Name),
            Address = NormalizeRequired(dto.Address),
            City = NormalizeRequired(dto.City)
        };

        await _repo.AddAsync(cinema, ct);
        return cinema.Id;
    }

    public async Task UpdateAsync(CinemaEditDto dto, CancellationToken ct = default)
    {
        if (dto.Id <= 0)
            throw new NotFoundDomainException("Cinema not found.");

        var cinema = await _repo.GetByIdAsync(dto.Id, asTracking: true, includeDeleted: false, ct);
        if (cinema == null)
            throw new NotFoundDomainException("Cinema not found.");

        cinema.Name = NormalizeRequired(dto.Name);
        cinema.Address = NormalizeRequired(dto.Address);
        cinema.City = NormalizeRequired(dto.City);

        await _repo.UpdateAsync(cinema, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var cinema = await _repo.GetByIdAsync(id, asTracking: true, includeDeleted: false, ct);
        if (cinema == null)
            throw new NotFoundDomainException("Cinema not found.");

        var hasHalls = await _repo.HasHallsAsync(id, ct);
        var hasSessions = await _repo.HasSessionsAsync(id, ct);

        if (hasHalls || hasSessions)
            throw new ConflictDomainException("Cannot delete cinema because it has halls or sessions.");

        await _repo.DeleteAsync(cinema, ct);
    }
}
