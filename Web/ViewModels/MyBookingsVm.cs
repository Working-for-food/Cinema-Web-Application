namespace Web.ViewModels.Bookings;

public class MyBookingsVm
{
    public List<Item> Items { get; set; } = new();

    public class Item
    {
        public int BookingId { get; set; }
        public string MovieTitle { get; set; } = "";
        public DateTime StartTime { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsCancelled { get; set; }
    }
}
