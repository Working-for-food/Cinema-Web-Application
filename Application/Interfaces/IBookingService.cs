using Application.DTOs;

public interface IBookingService
{
    Task<BookingResultDto> CreateAsync(string userId, BookingCreateDto dto, CancellationToken ct);
    Task<BookingResultDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<BookingResultDto>> GetMyAsync(string userId, CancellationToken ct);
    Task CancelAsync(string userId, int bookingId, CancellationToken ct);
}
