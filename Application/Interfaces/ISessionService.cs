using Application.DTOs;
using Application.DTOs.Pricing;

namespace Application.Interfaces;

public interface ISessionService
{
    Task<SessionDetailsDto?> GetByIdAsync(int id, CancellationToken ct);

    Task<SessionEditDto?> GetForEditAsync(int id, CancellationToken ct);

    Task<PagedResult<SessionListDto>> GetAllPagedAsync(
        DateTime? from,
        DateTime? to,
        int? cinemaId,
        int? hallId,
        int? movieId,
        bool includeCancelled,
        bool includeFinished,
        string? sort,
        int page,
        CancellationToken ct);

    Task<int> CreateAsync(SessionEditDto dto, CancellationToken ct);
    Task<bool> UpdateAsync(int id, SessionEditDto dto, CancellationToken ct);
    Task<bool> CancelAsync(int id, CancellationToken ct);
    Task<bool> RestoreAsync(int id, CancellationToken ct);
    Task EnsureSessionSeatsCreatedAsync(int sessionId, CancellationToken ct);

    Task<bool> HasBookingsAsync(int sessionId, CancellationToken ct);

    Task<SessionPricingDto> GetPricingAsync(int sessionId, CancellationToken ct);
    Task ApplyPricingAsync(int sessionId, SessionPricingDto pricing, CancellationToken ct);

    Task<IReadOnlyList<SessionSeatPriceDto>> GetSeatPricesAsync(int sessionId, CancellationToken ct);

    Task<IReadOnlyList<SessionSeatDto>> GetSeatsForBookingAsync(int sessionId, CancellationToken ct);

}