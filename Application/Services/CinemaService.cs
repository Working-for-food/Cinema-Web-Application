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

    public async Task<List<CinemaListDto>> GetAllAsync(string? city = null, string? search = null, string? sort = null)
    {
        var cinemas = await _repo.GetAllAsync(city, search, sort);

        return cinemas.Select(c => new CinemaListDto
        {
            Id = c.Id,
            Name = c.Name,
            Address = c.Address,
            City = c.City ?? ""
        }).ToList();
    }

    public Task<List<string>> GetCitiesAsync()
        => _repo.GetCitiesAsync();

    public async Task<CinemaEditDto?> GetForEditAsync(int id)
    {
        var cinema = await _repo.GetByIdAsync(id);
        if (cinema == null) return null;

        return new CinemaEditDto
        {
            Id = cinema.Id,
            Name = cinema.Name,
            Address = cinema.Address,
            City = cinema.City ?? ""
        };
    }

    public async Task<CinemaDetailsDto?> GetDetailsAsync(int id)
    {
        var cinema = await _repo.GetByIdAsync(id);
        if (cinema == null) return null;

        return new CinemaDetailsDto
        {
            Id = cinema.Id,
            Name = cinema.Name,
            Address = cinema.Address,
            City = cinema.City ?? ""
        };
    }

    public async Task<int> CreateAsync(CinemaEditDto dto)
    {
        var cinema = new Cinema
        {
            Name = dto.Name.Trim(),
            Address = dto.Address.Trim(),
            City = dto.City.Trim()
        };

        await _repo.AddAsync(cinema);
        return cinema.Id;
    }

    public async Task UpdateAsync(CinemaEditDto dto)
    {
        if (dto.Id <= 0)
            throw new NotFoundDomainException("Cinema not found.");

        var cinema = await _repo.GetByIdAsync(dto.Id);
        if (cinema == null)
            throw new NotFoundDomainException("Cinema not found.");

        cinema.Name = dto.Name.Trim();
        cinema.Address = dto.Address.Trim();
        cinema.City = dto.City.Trim();

        await _repo.UpdateAsync(cinema);
    }

    public async Task DeleteAsync(int id)
    {
        var cinema = await _repo.GetByIdAsync(id);
        if (cinema == null)
            throw new NotFoundDomainException("Cinema not found.");

        var hasHalls = await _repo.HasHallsAsync(id);
        var hasSessions = await _repo.HasSessionsAsync(id);

        if (hasHalls || hasSessions)
            throw new ConflictDomainException("Cannot delete cinema because it has halls or sessions.");

        await _repo.DeleteAsync(cinema);
    }
}
