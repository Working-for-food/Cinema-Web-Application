using Application.DTOs;
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

    public async Task<List<CinemaListVm>> GetAllAsync()
    {
        var cinemas = await _repo.GetAllAsync();
        return cinemas.Select(c => new CinemaListVm
        {
            Id = c.Id,
            Name = c.Name,
            Address = c.Address,
            City = c.City ?? ""
        }).ToList();
    }

    public async Task<CinemaEditVm?> GetForEditAsync(int id)
    {
        var cinema = await _repo.GetByIdAsync(id);
        if (cinema == null) return null;

        return new CinemaEditVm
        {
            Id = cinema.Id,
            Name = cinema.Name,
            Address = cinema.Address,
            City = cinema.City ?? ""
        };
    }

    public async Task<int> CreateAsync(CinemaEditVm vm)
    {
        Validate(vm);

        var cinema = new Cinema
        {
            Name = vm.Name.Trim(),
            Address = vm.Address.Trim(),
            City = vm.City.Trim()
        };

        await _repo.AddAsync(cinema);
        return cinema.Id;
    }

    public async Task<bool> UpdateAsync(CinemaEditVm vm)
    {
        if (vm.Id <= 0) return false;

        Validate(vm);

        var cinema = await _repo.GetByIdAsync(vm.Id);
        if (cinema == null) return false;

        cinema.Name = vm.Name.Trim();
        cinema.Address = vm.Address.Trim();
        cinema.City = vm.City.Trim();

        await _repo.UpdateAsync(cinema);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var cinema = await _repo.GetByIdAsync(id);
        if (cinema == null) return false;

        await _repo.DeleteAsync(cinema);
        return true;
    }

    private static void Validate(CinemaEditVm vm)
    {
        if (string.IsNullOrWhiteSpace(vm.Name))
            throw new ArgumentException("Cinema name is required.");

        if (string.IsNullOrWhiteSpace(vm.Address))
            throw new ArgumentException("Cinema address is required.");

        if (string.IsNullOrWhiteSpace(vm.City))
            throw new ArgumentException("Cinema city is required.");
    }
}
