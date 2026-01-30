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
            RowNumber = s.RowNumber,
            SeatNumber = s.SeatNumber
        }).ToList();
    }

    public async Task<GenerateSeatsDto?> GetSeatingAsync(int hallId, int? rows = null, int? seatsPerRow = null)
    {
        var hall = await _halls.GetByIdWithCinemaAsync(hallId);
        if (hall == null) return null;

        var already = await _seats.AnyForHallAsync(hallId);

        string? lockReason = null;
        var hasBookings = await _halls.HasAnyBookingsAsync(hallId);
        var hasSessions = await _halls.HasAnySessionsAsync(hallId);

        if (hasBookings)
            lockReason = "Неможливо змінювати місця: у цьому залі є бронювання на сеанси.";
        else if (hasSessions)
            lockReason = "Неможливо змінювати місця: у цьому залі вже є сеанси.";

        var canEdit = string.IsNullOrEmpty(lockReason) && !already;

        var r = rows ?? 10;
        var spr = seatsPerRow ?? 12;

        var dto = new GenerateSeatsDto
        {
            HallId = hallId,
            HallName = $"{hall.Cinema?.Name} - {hall.Name}",
            Rows = r,
            SeatsPerRow = spr,
            AlreadyGenerated = already,
            CanEditSeats = canEdit,
            LockReason = lockReason
        };

        if (already)
        {
            var seats = await _seats.GetByHallAsync(hallId);
            dto.Seats = seats.Select(s => new SeatDto
            {
                RowNumber = s.RowNumber,
                SeatNumber = s.SeatNumber
            }).ToList();
        }
        else
        {
            dto.RowConfigs = Enumerable.Range(1, r)
                .Select(i => new RowSeatsDto { RowNumber = i, SeatsCount = spr })
                .ToList();
        }

        return dto;
    }

    public async Task GenerateSeatsByConfigAsync(int hallId, List<RowSeatsDto> rows, bool allowRegenerate)
    {
        if (!await _halls.ExistsAsync(hallId))
            throw new InvalidOperationException("Зал не знайдено.");

        await EnsureHallCanBeModifiedAsync(hallId);

        if (rows == null || rows.Count == 0)
        {
            if (!allowRegenerate)
                throw new ArgumentException("Конфігурація рядів порожня.");

            var existing = await _seats.GetByHallAsync(hallId);

            rows = existing
                .GroupBy(s => s.RowNumber)
                .Select(g => new RowSeatsDto { RowNumber = g.Key, SeatsCount = g.Count() })
                .OrderBy(x => x.RowNumber)
                .ToList();

            if (rows.Count == 0)
                throw new ArgumentException("Неможливо повторно створити місця: поточні місця відсутні.");
        }

        // дедуп по номеру ряду
        rows = rows
            .GroupBy(r => r.RowNumber)
            .Select(g => new RowSeatsDto { RowNumber = g.Key, SeatsCount = g.Max(x => x.SeatsCount) })
            .OrderBy(x => x.RowNumber)
            .ToList();

        foreach (var r in rows)
        {
            if (r.RowNumber < 1) throw new ArgumentException("Номер ряду має бути >= 1.");
            if (r.SeatsCount < 1) throw new ArgumentException("Кількість місць у ряду має бути >= 1.");
        }

        var already = await _seats.AnyForHallAsync(hallId);

        if (already && !allowRegenerate)
            throw new InvalidOperationException("Місця вже згенеровані. Повторне створення можливе лише після підтвердження.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            if (already && allowRegenerate)
                await _seats.DeleteByHallAsync(hallId);

            var list = new List<Seat>();
            foreach (var row in rows)
            {
                for (int n = 1; n <= row.SeatsCount; n++)
                {
                    list.Add(new Seat
                    {
                        HallId = hallId,
                        RowNumber = row.RowNumber,
                        SeatNumber = n,
                        Category = SeatCategory.Standard
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

    private async Task ValidateHallAsync(HallEditDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Назва залу є обов’язковою.");

        if (!await CinemaExistsAsync(dto.CinemaId))
            throw new ArgumentException("Кінотеатр не знайдено.");
    }

    private Task<bool> CinemaExistsAsync(int cinemaId)
        => _db.Cinemas.AnyAsync(c => c.Id == cinemaId && !c.IsDeleted);

    private async Task EnsureHallCanBeDeletedAsync(int hallId)
    {
        if (await _halls.HasAnyBookingsAsync(hallId))
            throw new InvalidOperationException("Неможливо видалити зал: у цьому залі є бронювання на сеанси.");

        if (await _halls.HasAnySessionsAsync(hallId))
            throw new InvalidOperationException("Неможливо видалити зал: у цьому залі вже є сеанси.");
    }

    private async Task EnsureHallCanBeModifiedAsync(int hallId)
    {
        if (await _halls.HasAnyBookingsAsync(hallId))
            throw new InvalidOperationException("Неможливо змінювати місця: у цьому залі є бронювання на сеанси.");

        if (await _halls.HasAnySessionsAsync(hallId))
            throw new InvalidOperationException("Неможливо змінювати місця: у цьому залі вже є сеанси.");
    }
}
