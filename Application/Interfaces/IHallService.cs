using Application.DTOs;

namespace Application.Interfaces;

public interface IHallService
{
    Task<List<HallListDto>> GetAllAsync();
    Task<List<HallListDto>> GetByCinemaAsync(int cinemaId);

    Task<HallEditDto?> GetForEditAsync(int hallId);
    Task CreateAsync(HallEditDto dto);
    Task UpdateAsync(HallEditDto dto);
    Task DeleteAsync(int hallId);

    Task<bool> SeatsAlreadyGeneratedAsync(int hallId);
    Task<List<SeatDto>> GetSeatsAsync(int hallId);

    Task<List<SeatDto>> GenerateSeatsAsync(
        int hallId,
        int rows,
        int seatsPerRow,
        bool allowRegenerate);
}
