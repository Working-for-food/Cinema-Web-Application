using Application.DTOs;

namespace Application.Interfaces;

public interface ICinemaService
{
    Task<List<CinemaListVm>> GetAllAsync();
    Task<CinemaEditVm?> GetForEditAsync(int id);
    Task<int> CreateAsync(CinemaEditVm vm);
    Task<bool> UpdateAsync(CinemaEditVm vm);
    Task<bool> DeleteAsync(int id);
}
