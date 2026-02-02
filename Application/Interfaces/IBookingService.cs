public interface IBookingService
{
    Task<BookingResultDto> CreateAsync(string userId, BookingCreateDto dto, CancellationToken ct = default);
    Task CancelAsync(string userId, int bookingId, CancellationToken ct = default); // optional
}
