using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Entities; 
using Infrastructure.Interfaces;

namespace Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repo;

    public BookingService(IBookingRepository repo)
    {
        _repo = repo;
    }

    public async Task<BookingResultDto> CreateAsync(string userId, BookingCreateDto dto, CancellationToken ct)
    {
        var booking = await _repo.CreateBookingAsync(userId, dto.SessionId, dto.SeatIds, ct);

        return MapToDto(booking);
    }

    public async Task<BookingResultDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var booking = await _repo.GetByIdAsync(id, ct);
        if (booking is null) return null;

        return MapToDto(booking);
    }

    public async Task<IReadOnlyList<BookingResultDto>> GetMyAsync(string userId, CancellationToken ct)
    {
        var items = await _repo.GetMyBookingsAsync(userId, ct);
        return items.Select(MapToDto).ToList();
    }

    public async Task CancelAsync(string userId, int bookingId, CancellationToken ct)
    {
        await _repo.CancelBookingAsync(userId, bookingId, ct);
    }

    private static BookingResultDto MapToDto(Booking b)
    {
        return new BookingResultDto
        {
            BookingId = b.Id,
            SessionId = b.SessionId,
            TotalAmount = b.TotalAmount,
            BookedAt = b.BookedAt,

            MovieTitle = b.Session?.Movie?.Title ?? "Невідомий фільм",
            MoviePosterPath = b.Session?.Movie?.PosterPath,

            StartTime = b.Session?.StartTime ?? DateTime.MinValue,
            EndTime = b.Session?.EndTime ?? DateTime.MinValue,
            PresentationType = b.Session?.PresentationType.ToString() ?? "2D",

            HallName = b.Session?.Hall?.Name ?? "Зал не вказано",
            CinemaName = b.Session?.Hall?.Cinema?.Name ?? "Кінотеатр не вказано",
            CinemaAddress = b.Session?.Hall?.Cinema?.Address ?? "Адреса не вказана",

            Seats = b.SessionSeats
                .OrderBy(x => x.Seat.RowNumber)
                .ThenBy(x => x.Seat.SeatNumber)
                .Select(x => new BookedSeatDto
                {
                    SeatId = x.SeatId,
                    RowNumber = x.Seat.RowNumber,
                    SeatNumber = x.Seat.SeatNumber,
                    Category = (int)x.Seat.Category,
                    Price = x.Price
                })
                .ToList()
        };
    }
}