using Application.DTOs;

namespace Application.Interfaces;

public interface IBookingService
{
    Task<BookingResultDto> CreateAsync(string userId, BookingCreateDto dto, CancellationToken ct);
    Task<BookingResultDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<BookingResultDto>> GetMyAsync(string userId, CancellationToken ct);
}
