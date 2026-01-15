namespace Infrastructure.Entities;

public class SessionSeat
{
    public int Id { get; set; }

    public int SessionId { get; set; }
    public int SeatId { get; set; }

    public int? BookingId { get; set; }

    public decimal Price { get; set; } // decimal(10,2)

    public Session Session { get; set; } = null!;
    public Seat Seat { get; set; } = null!;
    public Booking? Booking { get; set; }
}
