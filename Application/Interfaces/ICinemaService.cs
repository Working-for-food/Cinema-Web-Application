using Application.DTOs;

namespace Application.Interfaces;

public interface ICinemaService
{
    Task<List<CinemaListDto>> GetAllAsync(string? city = null, string? search = null, string? sort = null, CancellationToken ct = default);
    Task<List<string>> GetCitiesAsync(CancellationToken ct = default);

    Task<CinemaEditDto?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<CinemaDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default);

    Task<int> CreateAsync(CinemaEditDto dto, CancellationToken ct = default);
    Task UpdateAsync(CinemaEditDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
