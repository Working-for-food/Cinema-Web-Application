using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly CinemaDbContext _db;

    public BookingRepository(CinemaDbContext db)
    {
        _db = db;
    }

    public async Task<Booking?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _db.Bookings
            .Include(b => b.SessionSeats)
                .ThenInclude(ss => ss.Seat)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<IReadOnlyList<Booking>> GetMyBookingsAsync(string userId, CancellationToken ct)
    {
        return await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Session)
                .ThenInclude(s => s.Movie)
            .Include(b => b.Session)
                .ThenInclude(s => s.Hall)
                    .ThenInclude(h => h.Cinema)
            .Include(b => b.SessionSeats)
                .ThenInclude(ss => ss.Seat)
            .Where(b => b.UserId == userId && !b.IsDeleted)
            .OrderByDescending(b => b.BookedAt)
            .ToListAsync(ct);
    }

    public async Task<Booking> CreateBookingAsync(string userId, int sessionId, IReadOnlyList<int> seatIds, CancellationToken ct)
    {
        if (seatIds.Count == 0)
            throw new InvalidOperationException("Оберіть хоча б одне місце.");

        // Важно: транзакция, чтобы check + update были атомарными
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct);
        if (session is null || session.IsCancelled)
            throw new InvalidOperationException("Сеанс не знайдено або його скасовано.");

        // Берём только выбранные места для этого сеанса
        var sessionSeats = await _db.SessionSeats
            .Include(x => x.Seat)
            .Where(x => x.SessionId == sessionId && seatIds.Contains(x.SeatId))
            .ToListAsync(ct);

        if (sessionSeats.Count != seatIds.Count)
            throw new InvalidOperationException("Деякі місця некоректні для цього сеансу.");

        // Проверка занятости
        if (sessionSeats.Any(x => x.BookingId != null))
            throw new InvalidOperationException("Деякі місця вже зайняті. Оновіть сторінку та спробуйте ще раз.");

        var booking = new Booking
        {
            UserId = userId,
            SessionId = sessionId,
            BookedAt = DateTime.UtcNow,
            TotalAmount = 0m
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct); // получить booking.Id

        foreach (var ss in sessionSeats)
            ss.BookingId = booking.Id;

        booking.TotalAmount = sessionSeats.Sum(x => x.Price);

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return booking;
    }
}
