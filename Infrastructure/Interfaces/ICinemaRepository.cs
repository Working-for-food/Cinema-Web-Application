using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface ICinemaRepository
{
    Task<List<Cinema>> GetAllAsync(string? city = null, string? search = null, string? sort = null);
    Task<Cinema?> GetByIdAsync(int id);

    Task AddAsync(Cinema cinema);
    Task UpdateAsync(Cinema cinema);

    Task DeleteAsync(Cinema cinema);

    Task<bool> ExistsAsync(int id);

    Task<bool> HasHallsAsync(int cinemaId);
    Task<bool> HasSessionsAsync(int cinemaId);

    Task<List<string>> GetCitiesAsync();
}
