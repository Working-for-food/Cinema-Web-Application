using Infrastructure.Entities;

namespace Web.ViewModels.Bookings;

public class BookingCreateVm
{
    public int SessionId { get; set; }

    public string MovieTitle { get; set; } = "";
    public string CinemaName { get; set; } = "";
    public string HallName { get; set; } = "";

    public DateTime StartTime { get; set; }
    public PresentationType PresentationType { get; set; }

    // В будущем сюда подставишь реальные места из SessionSeats
    public List<SeatVm> Seats { get; set; } = new();

    // выбранные места (важно: name="SeatIds" во view)
    public List<int> SeatIds { get; set; } = new();

    public class SeatVm
    {
        public int SeatId { get; set; }
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
        public SeatCategory Category { get; set; }
        public decimal Price { get; set; }
        public bool IsBooked { get; set; } // true = disabled
    }
}
