using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface IHallRepository
{
    Task<List<Hall>> GetAllAsync();
    Task<List<Hall>> GetByCinemaAsync(int cinemaId);
    Task<Hall?> GetByIdAsync(int id);

    Task AddAsync(Hall hall);
    Task UpdateAsync(Hall hall);
    Task DeleteAsync(Hall hall);

    Task<bool> ExistsAsync(int hallId);
}
