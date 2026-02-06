namespace Infrastructure.Entities;

public class SessionRowPrice
{
    public int SessionId { get; set; }
    public int RowNumber { get; set; }
    public decimal BasePrice { get; set; }

    public Session Session { get; set; } = null!;
}