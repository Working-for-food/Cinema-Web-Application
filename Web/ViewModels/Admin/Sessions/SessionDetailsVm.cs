using Infrastructure.Entities;

namespace Web.ViewModels.Admin.Sessions;

public class SessionDetailsVm
{
    public int Id { get; set; }
    public string MovieTitle { get; set; } = "";
    public string CinemaName { get; set; } = "";
    public string HallName { get; set; } = "";

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public PresentationType PresentationType { get; set; }
    public bool IsCancelled { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Booking
    public int TotalSeats { get; set; }
    public int BookedSeats { get; set; }
    public int FreeSeats { get; set; }
    public int BookingsCount { get; set; }

    public decimal TotalRevenue { get; set; }
    public decimal PotentialRevenue { get; set; }
    public decimal RemainingPotentialRevenue { get; set; }

    public decimal OccupancyPercent { get; set; } // 0..100
    public decimal AverageTicketPrice { get; set; }
    public decimal AverageBookingAmount { get; set; }

    public List<SeatVm> Seats { get; set; } = new();
    public List<CategoryStatVm> CategoryStats { get; set; } = new();
    public List<RowStatVm> RowStats { get; set; } = new();

    public sealed class SeatVm
    {
        public int SeatId { get; set; }
        public int Row { get; set; }
        public int Number { get; set; }
        public SeatCategory Category { get; set; }
        public decimal Price { get; set; }
        public int? BookingId { get; set; }

        public bool IsBooked => BookingId.HasValue;
    }

    public sealed class CategoryStatVm
    {
        public SeatCategory Category { get; set; }
        public int Total { get; set; }
        public int Booked { get; set; }
        public int Free { get; set; }
        public decimal Revenue { get; set; }
        public decimal PotentialRevenue { get; set; }
    }

    public sealed class RowStatVm
    {
        public int Row { get; set; }
        public int Total { get; set; }
        public int Booked { get; set; }
        public int Free { get; set; }
        public decimal Revenue { get; set; }
        public decimal PotentialRevenue { get; set; }
        public decimal OccupancyPercent { get; set; }
    }
}
