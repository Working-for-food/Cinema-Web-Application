using Web.ViewModels.Halls;

namespace Web.Services;

public interface IHallService
{
    Task<List<HallListVm>> GetAllAsync();
    Task<List<HallListVm>> GetByCinemaAsync(int cinemaId);

    Task<HallEditVm?> GetForEditAsync(int hallId);
    Task CreateAsync(HallEditVm vm);
    Task UpdateAsync(HallEditVm vm);
    Task DeleteAsync(int hallId);

    Task<bool> SeatsAlreadyGeneratedAsync(int hallId);
    Task<List<SeatVm>> GetSeatsAsync(int hallId);

    Task<List<SeatVm>> GenerateSeatsAsync(int hallId, int rows, int seatsPerRow, bool allowRegenerate);
}
