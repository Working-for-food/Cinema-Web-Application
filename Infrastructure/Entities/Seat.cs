namespace Infrastructure.Entities;
public enum SeatCategory
{
    Standard = 0,
    Vip = 1,
    Accessible = 2
}
public class Seat
{
    public int Id { get; set; }

    public int HallId { get; set; }
    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }
    public SeatCategory Category { get; set; } = SeatCategory.Standard;
    public Hall Hall { get; set; } = null!;
    public ICollection<SessionSeat> SessionSeats { get; set; } = new List<SessionSeat>();
}
