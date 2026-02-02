using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

public class BookingService : IBookingService
{
    private readonly CinemaDbContext _db;

    public BookingService(CinemaDbContext db)
    {
        _db = db;
    }

    public async Task<BookingResultDto> CreateAsync(string userId, BookingCreateDto dto, CancellationToken ct = default)
    {
        if (dto.SeatIds == null || dto.SeatIds.Count == 0)
            throw new ArgumentException("Choose at least one seat.");

        // 1) Проверим, что сессия существует и не отменена
        var session = await _db.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == dto.SessionId, ct);

        if (session == null) throw new InvalidOperationException("Session not found.");
        if (session.IsCancelled) throw new InvalidOperationException("Session is cancelled.");

        // 2) Транзакция: проверка + захват мест + создание Booking
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        // Достаём SessionSeat по выбранным seatIds
        var sessionSeats = await _db.SessionSeats
            .Include(ss => ss.Seat)
            .Where(ss => ss.SessionId == dto.SessionId && dto.SeatIds.Contains(ss.SeatId))
            .ToListAsync(ct);

        // Если не найдены все места — значит SessionSeat ещё не создан или seatId неверные
        if (sessionSeats.Count != dto.SeatIds.Count)
            throw new InvalidOperationException("Some seats are missing for this session.");

        // 2.1 Проверяем, что они свободны
        var busy = sessionSeats.Where(x => x.BookingId != null).ToList();
        if (busy.Count > 0)
            throw new InvalidOperationException("Some seats are already booked.");

        // 2.2 Считаем сумму
        var total = sessionSeats.Sum(x => x.Price);

        // 2.3 Создаём бронирование
        var booking = new Booking
        {
            UserId = userId,
            SessionId = dto.SessionId,
            TotalAmount = total,
            BookedAt = DateTime.UtcNow
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct); // получаем booking.Id

        // 2.4 “Захватываем” места: проставляем BookingId
        foreach (var ss in sessionSeats)
            ss.BookingId = booking.Id;

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new BookingResultDto
        {
            BookingId = booking.Id,
            SessionId = booking.SessionId,
            TotalAmount = booking.TotalAmount,
            BookedAt = booking.BookedAt,
            Seats = sessionSeats
                .OrderBy(x => x.Seat.RowNumber).ThenBy(x => x.Seat.SeatNumber)
                .Select(x => (x.SeatId, x.Seat.RowNumber, x.Seat.SeatNumber, x.Price))
                .ToList()
        };
    }

    public async Task CancelAsync(string userId, int bookingId, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var booking = await _db.Bookings
            .Include(b => b.SessionSeats)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

        if (booking == null) throw new InvalidOperationException("Booking not found.");
        if (booking.UserId != userId) throw new UnauthorizedAccessException();

        if (booking.IsDeleted) return;

        // освобождаем места
        foreach (var ss in booking.SessionSeats)
            ss.BookingId = null;

        booking.IsDeleted = true;
        booking.DeletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
