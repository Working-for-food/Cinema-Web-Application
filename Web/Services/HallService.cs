using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Web.ViewModels.Halls;

namespace Web.Services;

public class HallService : IHallService
{
    private readonly IHallRepository _halls;
    private readonly ISeatRepository _seats;
    private readonly ICinemaRepository _cinemas;

    public HallService(IHallRepository halls, ISeatRepository seats, ICinemaRepository cinemas)
    {
        _halls = halls;
        _seats = seats;
        _cinemas = cinemas;
    }

    public async Task<List<HallListVm>> GetAllAsync()
    {
        var halls = await _halls.GetAllAsync();

        // seats count можна рахувати через Include Seats, але у тебе репо halls без Seats.
        // Найпростіше: для Index показати 0 або зробити окремий підрахунок.
        // Я зроблю простий варіант: SeatsCount = h.Seats.Count якщо Include додаси.
        // Тут — без Seats: покажемо 0, або можеш підняти include.
        return halls.Select(h => new HallListVm
        {
            HallId = h.Id,
            HallName = h.Name,
            CinemaName = h.Cinema?.Name ?? $"Cinema #{h.CinemaId}",
            SeatsCount = h.Seats?.Count ?? 0
        }).ToList();
    }

    public async Task<List<HallListVm>> GetByCinemaAsync(int cinemaId)
    {
        var cinema = await _cinemas.GetByIdAsync(cinemaId);
        if (cinema == null) throw new ArgumentException("CinemaId does not exist");

        var halls = await _halls.GetByCinemaAsync(cinemaId);

        return halls.Select(h => new HallListVm
        {
            HallId = h.Id,
            HallName = h.Name,
            CinemaName = cinema.Name,
            SeatsCount = h.Seats?.Count ?? 0
        }).ToList();
    }

    public async Task<HallEditVm?> GetForEditAsync(int hallId)
    {
        var hall = await _halls.GetByIdAsync(hallId);
        if (hall == null) return null;

        return new HallEditVm
        {
            Id = hall.Id,
            CinemaId = hall.CinemaId,
            Name = hall.Name
        };
    }

    public async Task CreateAsync(HallEditVm vm)
    {
        await ValidateHall(vm);

        var hall = new Hall
        {
            CinemaId = vm.CinemaId,
            Name = vm.Name.Trim()
        };

        await _halls.AddAsync(hall);
    }

    public async Task UpdateAsync(HallEditVm vm)
    {
        if (vm.Id == null) throw new ArgumentException("Hall id is required");
        await ValidateHall(vm);

        var hall = await _halls.GetByIdAsync(vm.Id.Value);
        if (hall == null) throw new InvalidOperationException("Hall not found");

        hall.CinemaId = vm.CinemaId;
        hall.Name = vm.Name.Trim();

        await _halls.UpdateAsync(hall);
    }

    public async Task DeleteAsync(int hallId)
    {
        var hall = await _halls.GetByIdAsync(hallId);
        if (hall == null) return;

        // Seats каскадно видаляться через EF конфіг, але ок — явно почистимо.
        await _seats.DeleteByHallAsync(hallId);

        await _halls.DeleteAsync(hall);
    }

    public Task<bool> SeatsAlreadyGeneratedAsync(int hallId)
        => _seats.AnyForHallAsync(hallId);

    public async Task<List<SeatVm>> GetSeatsAsync(int hallId)
    {
        var seats = await _seats.GetByHallAsync(hallId);
        return seats.Select(s => new SeatVm
        {
            RowNumber = s.RowNumber,
            SeatNumber = s.SeatNumber
        }).ToList();
    }

    public async Task<List<SeatVm>> GenerateSeatsAsync(int hallId, int rows, int seatsPerRow, bool allowRegenerate)
    {
        if (rows <= 0) throw new ArgumentException("Rows must be > 0");
        if (seatsPerRow <= 0) throw new ArgumentException("SeatsPerRow must be > 0");

        if (!await _halls.ExistsAsync(hallId))
            throw new InvalidOperationException("Hall not found");

        var already = await _seats.AnyForHallAsync(hallId);
        if (already && !allowRegenerate)
            throw new InvalidOperationException("Seats already generated. Regenerate is not allowed.");

        if (already && allowRegenerate)
            await _seats.DeleteByHallAsync(hallId);

        var list = new List<Seat>(rows * seatsPerRow);

        for (int r = 1; r <= rows; r++)
        {
            for (int n = 1; n <= seatsPerRow; n++)
            {
                list.Add(new Seat
                {
                    HallId = hallId,
                    RowNumber = r,
                    SeatNumber = n,
                    Category = SeatCategory.Standard
                });
            }
        }

        // У тебе є unique index (HallId, RowNumber, SeatNumber),
        // тому якщо не видалити старі — буде DbUpdateException.
        await _seats.AddRangeAsync(list);

        return list.Select(s => new SeatVm { RowNumber = s.RowNumber, SeatNumber = s.SeatNumber }).ToList();
    }

    private async Task ValidateHall(HallEditVm vm)
    {
        if (string.IsNullOrWhiteSpace(vm.Name))
            throw new ArgumentException("Name is required");

        if (!await _cinemas.ExistsAsync(vm.CinemaId))
            throw new ArgumentException("CinemaId does not exist");
    }
}
