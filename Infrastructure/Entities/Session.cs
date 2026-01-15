namespace Infrastructure.Entities;

public class Session
{
    public int Id { get; set; }

    public int MovieId { get; set; }
    public int HallId { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Movie Movie { get; set; } = null!;
    public Hall Hall { get; set; } = null!;

    public ICollection<SessionSeat> SessionSeats { get; set; } = new List<SessionSeat>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
