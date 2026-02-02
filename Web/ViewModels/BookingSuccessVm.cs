namespace Web.ViewModels.Bookings;

public class BookingSuccessVm
{
    public int BookingId { get; set; }
    public int SessionId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime BookedAt { get; set; }

    public string MovieTitle { get; set; } = "";
    public string CinemaName { get; set; } = "";
    public string HallName { get; set; } = "";

    public List<string> SeatsText { get; set; } = new(); // "Ряд 3, Місце 7"
}
