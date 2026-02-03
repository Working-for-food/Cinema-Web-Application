using Application.DTOs;
using Application.Interfaces;
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

        return new BookingResultDto
        {
            BookingId = booking.Id,
            SessionId = booking.SessionId,
            TotalAmount = booking.TotalAmount,
            BookedAt = booking.BookedAt,
            Seats = booking.SessionSeats
                .OrderBy(x => x.Seat.RowNumber).ThenBy(x => x.Seat.SeatNumber)
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

    public async Task<BookingResultDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var b = await _repo.GetByIdAsync(id, ct);
        if (b is null) return null;

        return new BookingResultDto
        {
            BookingId = b.Id,
            SessionId = b.SessionId,
            TotalAmount = b.TotalAmount,
            BookedAt = b.BookedAt,
            Seats = b.SessionSeats
                .OrderBy(x => x.Seat.RowNumber).ThenBy(x => x.Seat.SeatNumber)
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

    public async Task<IReadOnlyList<BookingResultDto>> GetMyAsync(string userId, CancellationToken ct)
    {
        var items = await _repo.GetMyBookingsAsync(userId, ct);

        return items.Select(b => new BookingResultDto
        {
            BookingId = b.Id,
            SessionId = b.SessionId,
            TotalAmount = b.TotalAmount,
            BookedAt = b.BookedAt,
            Seats = b.SessionSeats
                .OrderBy(x => x.Seat.RowNumber).ThenBy(x => x.Seat.SeatNumber)
                .Select(x => new BookedSeatDto
                {
                    SeatId = x.SeatId,
                    RowNumber = x.Seat.RowNumber,
                    SeatNumber = x.Seat.SeatNumber,
                    Category = (int)x.Seat.Category,
                    Price = x.Price
                })
                .ToList()
        }).ToList();
    }
}
