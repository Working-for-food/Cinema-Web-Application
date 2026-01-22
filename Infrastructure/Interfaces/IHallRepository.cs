using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface IHallRepository
{
    Task<List<Hall>> GetAllWithCinemaAsync();
    Task<List<Hall>> GetByCinemaWithCinemaAsync(int cinemaId);

    Task<Hall?> GetByIdAsync(int id);
    Task<Hall?> GetByIdWithCinemaAsync(int id);

    Task AddAsync(Hall hall);
    Task UpdateAsync(Hall hall);
    Task DeleteAsync(Hall hall);

    Task<bool> ExistsAsync(int hallId);

    Task<bool> HasAnySessionsAsync(int hallId);
    Task<bool> HasAnyBookingsAsync(int hallId);
}
