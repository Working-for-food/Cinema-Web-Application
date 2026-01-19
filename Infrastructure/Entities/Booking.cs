namespace Infrastructure.Entities;

public class Booking
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    public int SessionId { get; set; }

    public decimal TotalAmount { get; set; } // decimal(10,2)
    public bool IsDeleted { get; set; } = false;
    public DateTime BookedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Session Session { get; set; } = null!;

    public ICollection<SessionSeat> SessionSeats { get; set; } = new List<SessionSeat>();
}
