using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Application.Services;

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

    public async Task<List<HallListDto>> GetAllAsync()
    {
        // Етап 2: одним запитом тягнемо Cinema через Include (в репозиторії)
        var halls = await _halls.GetAllWithCinemaAsync();

        // Якщо Seats не Include-яться, SeatsCount може бути 0.
        // (Ідеально — рахувати COUNT в DAL, але для етапів ок.)
        return halls.Select(h => new HallListDto
        {
            HallId = h.Id,
            HallName = h.Name,
            CinemaName = h.Cinema?.Name ?? $"Cinema #{h.CinemaId}",
            SeatsCount = (h.Seats != null) ? h.Seats.Count : 0
        }).ToList();
    }

    public async Task<List<HallListDto>> GetByCinemaAsync(int cinemaId)
    {
        if (!await _cinemas.ExistsAsync(cinemaId))
            throw new ArgumentException("CinemaId does not exist");

        var halls = await _halls.GetByCinemaWithCinemaAsync(cinemaId);

        return halls.Select(h => new HallListDto
        {
            HallId = h.Id,
            HallName = h.Name,
            CinemaName = h.Cinema?.Name ?? $"Cinema #{h.CinemaId}",
            SeatsCount = (h.Seats != null) ? h.Seats.Count : 0
        }).ToList();
    }

    public async Task<HallEditDto?> GetForEditAsync(int hallId)
    {
        var hall = await _halls.GetByIdWithCinemaAsync(hallId);
        if (hall == null) return null;

        return new HallEditDto
        {
            Id = hall.Id,
            CinemaId = hall.CinemaId,
            Name = hall.Name
        };
    }

    public async Task CreateAsync(HallEditDto dto)
    {
        await ValidateHallAsync(dto);

        var hall = new Hall
        {
            CinemaId = dto.CinemaId,
            Name = dto.Name.Trim()
        };

        await _halls.AddAsync(hall);
    }

    public async Task UpdateAsync(HallEditDto dto)
    {
        if (dto.Id == null)
            throw new ArgumentException("Hall id is required");

        await ValidateHallAsync(dto);

        var hall = await _halls.GetByIdAsync(dto.Id.Value);
        if (hall == null)
            throw new InvalidOperationException("Hall not found");

        hall.CinemaId = dto.CinemaId;
        hall.Name = dto.Name.Trim();

        await _halls.UpdateAsync(hall);
    }

    public async Task DeleteAsync(int hallId)
    {
        var hall = await _halls.GetByIdAsync(hallId);
        if (hall == null) return;

        // Якщо хочеш заборонити видалення залу при наявності сеансів/бронювань — можна тут теж.
        // Але по ТЗ етапу 2 головне — заборонити зміну seating при sessions/bookings.
        await _seats.DeleteByHallAsync(hallId);
        await _halls.DeleteAsync(hall);
    }

    public Task<bool> SeatsAlreadyGeneratedAsync(int hallId)
        => _seats.AnyForHallAsync(hallId);

    public async Task<List<SeatDto>> GetSeatsAsync(int hallId)
    {
        var seats = await _seats.GetByHallAsync(hallId);

        return seats.Select(s => new SeatDto
        {
            RowNumber = s.RowNumber,
            SeatNumber = s.SeatNumber
        }).ToList();
    }

    public async Task<List<SeatDto>> GenerateSeatsAsync(int hallId, int rows, int seatsPerRow, bool allowRegenerate)
    {
        if (rows < 1) throw new ArgumentException("Rows must be >= 1");
        if (seatsPerRow < 1) throw new ArgumentException("SeatsPerRow must be >= 1");

        if (!await _halls.ExistsAsync(hallId))
            throw new InvalidOperationException("Hall not found");

        // Етап 2 (обмеження): якщо є сеанси/бронювання — НЕ МОЖНА міняти seating
        if (await _halls.HasAnyBookingsAsync(hallId))
            throw new InvalidOperationException("Cannot change seats: there are bookings for sessions in this hall.");

        if (await _halls.HasAnySessionsAsync(hallId))
            throw new InvalidOperationException("Cannot change seats: this hall already has sessions.");

        var alreadyGenerated = await _seats.AnyForHallAsync(hallId);

        if (alreadyGenerated && !allowRegenerate)
            throw new InvalidOperationException("Seats already generated. Regenerate is not allowed.");

        if (alreadyGenerated && allowRegenerate)
            await _seats.DeleteByHallAsync(hallId);

        var list = new List<Seat>(rows * seatsPerRow);

        for (var r = 1; r <= rows; r++)
        {
            for (var n = 1; n <= seatsPerRow; n++)
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

        await _seats.AddRangeAsync(list);

        return list.Select(s => new SeatDto
        {
            RowNumber = s.RowNumber,
            SeatNumber = s.SeatNumber
        }).ToList();
    }

    private async Task ValidateHallAsync(HallEditDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required");

        if (!await _cinemas.ExistsAsync(dto.CinemaId))
            throw new ArgumentException("CinemaId does not exist");
    }
}
