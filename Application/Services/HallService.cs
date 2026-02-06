using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class HallService : IHallService
{
    private readonly IHallRepository _halls;
    private readonly ISeatRepository _seats;
    private readonly CinemaDbContext _db;

    public HallService(IHallRepository halls, ISeatRepository seats, CinemaDbContext db)
    {
        _halls = halls;
        _seats = seats;
        _db = db;
    }

    public async Task<List<HallListDto>> GetAllAsync()
    {
        var halls = await _halls.GetAllWithCinemaAsync();
        var counts = await _seats.CountByHallIdsAsync(halls.Select(h => h.Id));

        return halls.Select(h => new HallListDto
        {
            HallId = h.Id,
            HallName = h.Name,
            CinemaName = h.Cinema?.Name ?? $"Кінотеатр #{h.CinemaId}",
            SeatsCount = counts.TryGetValue(h.Id, out var c) ? c : 0
        }).ToList();
    }

    public async Task<List<HallListDto>> GetByCinemaAsync(int cinemaId)
    {
        if (!await CinemaExistsAsync(cinemaId))
            throw new ArgumentException("Кінотеатр не знайдено.");

        var halls = await _halls.GetByCinemaWithCinemaAsync(cinemaId);
        var counts = await _seats.CountByHallIdsAsync(halls.Select(h => h.Id));

        return halls.Select(h => new HallListDto
        {
            HallId = h.Id,
            HallName = h.Name,
            CinemaName = h.Cinema?.Name ?? $"Кінотеатр #{h.CinemaId}",
            SeatsCount = counts.TryGetValue(h.Id, out var c) ? c : 0
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
            Name = (dto.Name ?? string.Empty).Trim()
        };

        await _halls.AddAsync(hall);
    }

    public async Task UpdateAsync(HallEditDto dto)
    {
        if (dto.Id == null)
            throw new ArgumentException("Ідентифікатор залу є обов’язковим.");

        await ValidateHallAsync(dto);

        var hall = await _halls.GetByIdAsync(dto.Id.Value);
        if (hall == null)
            throw new InvalidOperationException("Зал не знайдено.");

        hall.CinemaId = dto.CinemaId;
        hall.Name = (dto.Name ?? string.Empty).Trim();

        await _halls.UpdateAsync(hall);
    }

    public async Task DeleteAsync(int hallId)
    {
        var hall = await _halls.GetByIdAsync(hallId);
        if (hall == null) return;

        await EnsureHallCanBeDeletedAsync(hallId);

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            await _seats.DeleteByHallAsync(hallId);
            await _halls.DeleteAsync(hall);

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public Task<bool> SeatsAlreadyGeneratedAsync(int hallId)
        => _seats.AnyForHallAsync(hallId);

    public async Task<List<SeatDto>> GetSeatsAsync(int hallId)
    {
        var seats = await _seats.GetByHallAsync(hallId);

        return seats.Select(s => new SeatDto
        {
            Id = s.Id,
            RowNumber = s.RowNumber,
            SeatNumber = s.SeatNumber,
            Category = s.Category
        }).ToList();
    }

    public async Task<GenerateSeatsDto?> GetSeatingAsync(int hallId, int? rows = null, int? seatsPerRow = null)
    {
        var hall = await _halls.GetByIdWithCinemaAsync(hallId);
        if (hall == null) return null;

        var already = await _seats.AnyForHallAsync(hallId);
        var lockReason = await GetHallLockReasonAsync(hallId);

        var r = NormalizePositive(rows, fallback: 10);
        var spr = NormalizePositive(seatsPerRow, fallback: 12);

        var dto = new GenerateSeatsDto
        {
            HallId = hallId,
            HallName = $"{hall.Cinema?.Name} - {hall.Name}",
            Rows = r,
            SeatsPerRow = spr,
            AlreadyGenerated = already,
            CanEditSeats = string.IsNullOrWhiteSpace(lockReason) && !already,
            LockReason = lockReason
        };

        if (already)
        {
            var seats = await _seats.GetByHallAsync(hallId);

            dto.Seats = seats.Select(s => new SeatDto
            {
                Id = s.Id,
                RowNumber = s.RowNumber,
                SeatNumber = s.SeatNumber,
                Category = s.Category
            }).ToList();

            dto.RowConfigs = seats
                .GroupBy(s => s.RowNumber)
                .Select(g => new RowSeatsDto
                {
                    RowNumber = g.Key,
                    SeatsCount = g.Count(),
                    Category = g.GroupBy(x => x.Category)
                                .OrderByDescending(x => x.Count())
                                .Select(x => x.Key)
                                .FirstOrDefault()
                })
                .OrderBy(x => x.RowNumber)
                .ToList();
        }
        else
        {
            dto.RowConfigs = Enumerable.Range(1, r)
                .Select(i => new RowSeatsDto
                {
                    RowNumber = i,
                    SeatsCount = spr,
                    Category = SeatCategory.Standard
                })
                .ToList();
        }

        return dto;
    }

    public async Task GenerateSeatsByConfigAsync(int hallId, List<RowSeatsDto> rows, bool allowRegenerate)
    {
        if (!await _halls.ExistsAsync(hallId))
            throw new InvalidOperationException("Зал не знайдено.");

        await EnsureHallCanBeModifiedAsync(hallId);

        var already = await _seats.AnyForHallAsync(hallId);

        if (already && !allowRegenerate)
            throw new InvalidOperationException("Місця вже згенеровані. Повторне створення можливе лише після підтвердження.");

        if ((rows == null || rows.Count == 0))
        {
            if (!allowRegenerate)
                throw new ArgumentException("Конфігурація рядів порожня.");

            var existing = await _seats.GetByHallAsync(hallId);

            rows = existing
                .GroupBy(s => s.RowNumber)
                .Select(g => new RowSeatsDto
                {
                    RowNumber = g.Key,
                    SeatsCount = g.Count(),
                    Category = g.GroupBy(x => x.Category)
                                .OrderByDescending(x => x.Count())
                                .Select(x => x.Key)
                                .FirstOrDefault()
                })
                .OrderBy(x => x.RowNumber)
                .ToList();

            if (rows.Count == 0)
                throw new ArgumentException("Неможливо повторно створити місця: поточні місця відсутні.");
        }

        rows = NormalizeRowConfigs(rows);
        ValidateRowConfigs(rows);

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            if (already && allowRegenerate)
                await _seats.DeleteByHallAsync(hallId);

            var list = new List<Seat>(rows.Sum(r => r.SeatsCount));

            foreach (var row in rows)
            {
                for (int n = 1; n <= row.SeatsCount; n++)
                {
                    list.Add(new Seat
                    {
                        HallId = hallId,
                        RowNumber = row.RowNumber,
                        SeatNumber = n,
                        Category = row.Category
                    });
                }
            }

            await _seats.AddRangeAsync(list);
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateRowCategoriesAsync(int hallId, List<RowCategoryDto> rows)
    {
        if (!await _halls.ExistsAsync(hallId))
            throw new InvalidOperationException("Зал не знайдено.");

        await EnsureHallCanBeModifiedAsync(hallId);

        if (rows == null || rows.Count == 0)
            throw new ArgumentException("Список рядів порожній.");

        var dict = rows
            .GroupBy(r => r.RowNumber)
            .ToDictionary(g => g.Key, g => g.Last().Category);

        foreach (var row in dict.Keys)
            if (row < 1) throw new ArgumentException("Номер ряду має бути >= 1.");

        await _seats.UpdateRowCategoriesAsync(hallId, dict);
    }

    public async Task UpdateSeatCategoriesAsync(int hallId, List<SeatCategoryChangeDto> seats)
    {
        if (!await _halls.ExistsAsync(hallId))
            throw new InvalidOperationException("Зал не знайдено.");

        await EnsureHallCanBeModifiedAsync(hallId);

        if (seats == null || seats.Count == 0)
            throw new ArgumentException("Список місць порожній.");

        var dict = seats
            .GroupBy(s => s.SeatId)
            .ToDictionary(g => g.Key, g => g.Last().Category);

        foreach (var seatId in dict.Keys)
            if (seatId < 1) throw new ArgumentException("Невірний SeatId.");

        await _seats.UpdateSeatCategoriesAsync(hallId, dict);
    }

    // ----------------- helpers -----------------

    private static int NormalizePositive(int? value, int fallback)
        => value.HasValue && value.Value > 0 ? value.Value : fallback;

    private static List<RowSeatsDto> NormalizeRowConfigs(List<RowSeatsDto> rows)
    {
        return rows
            .GroupBy(r => r.RowNumber)
            .Select(g =>
            {
                var best = g.OrderByDescending(x => x.SeatsCount).First();
                return new RowSeatsDto
                {
                    RowNumber = g.Key,
                    SeatsCount = best.SeatsCount,
                    Category = best.Category
                };
            })
            .OrderBy(x => x.RowNumber)
            .ToList();
    }

    private static void ValidateRowConfigs(List<RowSeatsDto> rows)
    {
        foreach (var r in rows)
        {
            if (r.RowNumber < 1)
                throw new ArgumentException("Номер ряду має бути >= 1.");

            if (r.SeatsCount < 1)
                throw new ArgumentException("Кількість місць у ряду має бути >= 1.");
        }
    }

    private async Task ValidateHallAsync(HallEditDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Назва залу є обов’язковою.");

        if (!await CinemaExistsAsync(dto.CinemaId))
            throw new ArgumentException("Кінотеатр не знайдено.");
    }

    private Task<bool> CinemaExistsAsync(int cinemaId)
        => _db.Cinemas.AnyAsync(c => c.Id == cinemaId && !c.IsDeleted);

    private async Task<string?> GetHallLockReasonAsync(int hallId)
    {
        if (await _halls.HasAnyBookingsAsync(hallId))
            return "Неможливо змінювати місця: у цьому залі є бронювання на сеанси.";

        if (await _halls.HasAnySessionsAsync(hallId))
            return "Неможливо змінювати місця: у цьому залі вже є сеанси.";

        return null;
    }

    private async Task EnsureHallCanBeDeletedAsync(int hallId)
    {
        var reason = await GetHallLockReasonAsync(hallId);
        if (!string.IsNullOrWhiteSpace(reason))
        {
            if (reason.Contains("бронювання"))
                throw new InvalidOperationException("Неможливо видалити зал: у цьому залі є бронювання на сеанси.");
            if (reason.Contains("сеанси"))
                throw new InvalidOperationException("Неможливо видалити зал: у цьому залі вже є сеанси.");
        }
    }

    private async Task EnsureHallCanBeModifiedAsync(int hallId)
    {
        var reason = await GetHallLockReasonAsync(hallId);
        if (!string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException(reason);
    }
}
