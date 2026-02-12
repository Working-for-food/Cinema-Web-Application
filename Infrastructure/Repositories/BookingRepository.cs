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
            .AsNoTracking()
            .Include(b => b.Session)
                .ThenInclude(s => s.Movie)
            .Include(b => b.Session)
                .ThenInclude(s => s.Hall)
                    .ThenInclude(h => h.Cinema)
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
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    public async Task<Booking> CreateBookingAsync(string userId, int sessionId, IReadOnlyList<int> seatIds, CancellationToken ct)
    {
        if (seatIds.Count == 0)
            throw new InvalidOperationException("Оберіть хоча б одне місце.");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var session = await _db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct);
            if (session is null || session.IsCancelled)
                throw new InvalidOperationException("Сеанс не знайдено або його скасовано.");

            var sessionSeats = await _db.SessionSeats
                .Include(x => x.Seat)
                .Where(x => x.SessionId == sessionId && seatIds.Contains(x.SeatId))
                .ToListAsync(ct);

            if (sessionSeats.Count != seatIds.Count)
                throw new InvalidOperationException("Деякі місця некоректні для цього сеансу.");

            if (sessionSeats.Any(x => x.BookingId != null))
                throw new InvalidOperationException("Деякі місця вже зайняті. Оновіть сторінку та спробуйте ще раз.");

            var booking = new Booking
            {
                UserId = userId,
                SessionId = sessionId,
                BookedAt = DateTime.UtcNow,
                IsDeleted = false,
                TotalAmount = sessionSeats.Sum(x => x.Price) 
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync(ct); 

            foreach (var ss in sessionSeats)
            {
                ss.BookingId = booking.Id;
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            var fullBooking = await GetByIdAsync(booking.Id, ct);

            return fullBooking ?? throw new InvalidOperationException("Не вдалося завантажити створене бронювання.");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task CancelBookingAsync(string userId, int bookingId, CancellationToken ct)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var booking = await _db.Bookings
                .Include(b => b.SessionSeats)
                .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

            if (booking is null || booking.IsDeleted)
                throw new InvalidOperationException("Бронювання не знайдено або вже скасовано.");

            if (booking.UserId != userId)
                throw new InvalidOperationException("Ви не можете скасувати чуже бронювання.");

            foreach (var ss in booking.SessionSeats)
            {
                ss.BookingId = null;
            }

            booking.IsDeleted = true;
            booking.DeletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}