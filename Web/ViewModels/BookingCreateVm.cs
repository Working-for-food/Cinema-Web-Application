namespace Web.ViewModels;

public class BookingCreateVm
{
    public int SessionId { get; set; }
    public List<SeatVm> Seats { get; set; } = new();

    public List<int>? SelectedSeatIds { get; set; }

    public class SeatVm
    {
        public int SeatId { get; set; }
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
        public int Category { get; set; }
        public decimal Price { get; set; }
        public bool IsBooked { get; set; }
    }
}
