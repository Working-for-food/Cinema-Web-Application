using Infrastructure.Entities;

namespace Infrastructure.Interfaces;
public interface ICinemaRepository
{
    Task<List<Cinema>> GetAllAsync(
        string? city = null,
        string? search = null,
        string? sort = null,
        bool includeDeleted = false,
        CancellationToken ct = default);

    Task<Cinema?> GetByIdAsync(
        int id,
        bool asTracking = true,
        bool includeDeleted = false,
        CancellationToken ct = default);

    Task AddAsync(Cinema cinema, CancellationToken ct = default);
    Task UpdateAsync(Cinema cinema, CancellationToken ct = default);
    Task DeleteAsync(Cinema cinema, CancellationToken ct = default);

    Task<bool> ExistsAsync(int id, bool includeDeleted = false, CancellationToken ct = default);

    Task<bool> HasHallsAsync(int cinemaId, CancellationToken ct = default);
    Task<bool> HasSessionsAsync(int cinemaId, CancellationToken ct = default);

    Task<List<string>> GetCitiesAsync(bool includeDeleted = false, CancellationToken ct = default);
}
