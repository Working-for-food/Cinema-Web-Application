using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface IBookingRepository
{
    Task<Booking> CreateBookingAsync(string userId, int sessionId, IReadOnlyList<int> seatIds, CancellationToken ct);
    Task<IReadOnlyList<Booking>> GetMyBookingsAsync(string userId, CancellationToken ct);
    Task<Booking?> GetByIdAsync(int id, CancellationToken ct);
    Task CancelBookingAsync(string userId, int bookingId, CancellationToken ct);
}
