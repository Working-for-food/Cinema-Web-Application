namespace Web.ViewModels;

public class BookingSuccessVm
{
    public int BookingId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime BookedAt { get; set; }
    public List<string> Seats { get; set; } = new();
}
