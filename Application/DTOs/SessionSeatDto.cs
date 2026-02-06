namespace Application.DTOs;

public sealed record SessionSeatDto
{
    public int SeatId { get; init; }
    public int RowNumber { get; init; }
    public int SeatNumber { get; init; }
    public int Category { get; init; }
    public decimal Price { get; init; }
    public int? BookingId { get; init; }
}