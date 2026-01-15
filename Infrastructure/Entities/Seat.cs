namespace Infrastructure.Entities;

public class Seat
{
    public int Id { get; set; }

    public int HallId { get; set; }
    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }

    public Hall Hall { get; set; } = null!;
    public ICollection<SessionSeat> SessionSeats { get; set; } = new List<SessionSeat>();
}
