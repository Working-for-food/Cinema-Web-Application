namespace Application.DTOs;

public class BookingResultDto
{
    public int BookingId { get; set; }
    public int SessionId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime BookedAt { get; set; }
    public string? MovieTitle { get; set; }
    public string? MoviePosterPath { get; set; }

    public List<BookedSeatDto> Seats { get; set; } = new();
}

public class BookedSeatDto
{
    public int SeatId { get; set; }
    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }
    public int Category { get; set; }
    public decimal Price { get; set; }
}
