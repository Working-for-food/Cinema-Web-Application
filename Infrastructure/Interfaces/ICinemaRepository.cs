using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface ICinemaRepository
{
    Task<List<Cinema>> GetAllAsync();
    Task<Cinema?> GetByIdAsync(int id);
    Task AddAsync(Cinema cinema);
    Task UpdateAsync(Cinema cinema);
    Task DeleteAsync(Cinema cinema);
    Task<bool> ExistsAsync(int id);
}
