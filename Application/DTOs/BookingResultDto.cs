public class BookingResultDto
{
    public int BookingId { get; set; }
    public int SessionId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime BookedAt { get; set; }
    public List<(int SeatId, int Row, int Seat, decimal Price)> Seats { get; set; } = new();
}
