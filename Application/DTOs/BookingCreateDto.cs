namespace Application.DTOs;

public class BookingCreateDto
{
    public int SessionId { get; set; }
    public List<int> SeatIds { get; set; } = new();
}
