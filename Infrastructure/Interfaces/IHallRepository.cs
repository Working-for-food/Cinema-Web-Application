using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface IHallRepository
{
    // Lists (оптимізовано: одразу з Cinema)
    Task<List<Hall>> GetAllWithCinemaAsync();
    Task<List<Hall>> GetByCinemaWithCinemaAsync(int cinemaId);

    // Edit
    Task<Hall?> GetByIdAsync(int id);              // якщо треба без include
    Task<Hall?> GetByIdWithCinemaAsync(int id);    // для Edit UI

    Task AddAsync(Hall hall);
    Task UpdateAsync(Hall hall);
    Task DeleteAsync(Hall hall);

    Task<bool> ExistsAsync(int hallId);

    // Етап 2: обмеження
    Task<bool> HasAnySessionsAsync(int hallId);
    Task<bool> HasAnyBookingsAsync(int hallId);
}
