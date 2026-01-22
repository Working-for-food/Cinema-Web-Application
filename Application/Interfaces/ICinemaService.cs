using Application.DTOs;

namespace Application.Interfaces;

public interface ICinemaService
{
    Task<List<CinemaListDto>> GetAllAsync(string? city = null, string? search = null, string? sort = null);
    Task<List<string>> GetCitiesAsync();

    Task<CinemaEditDto?> GetForEditAsync(int id);
    Task<CinemaDetailsDto?> GetDetailsAsync(int id);

    Task<int> CreateAsync(CinemaEditDto dto);
    Task UpdateAsync(CinemaEditDto dto);
    Task DeleteAsync(int id);
}
