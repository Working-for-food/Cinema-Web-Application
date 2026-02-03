namespace Infrastructure.Entities;

public class SessionCategoryMultiplier
{
    public int SessionId { get; set; }
    public SeatCategory Category { get; set; }
    public decimal Multiplier { get; set; }
    public Session Session { get; set; } = null!;
}