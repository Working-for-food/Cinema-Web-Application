namespace Web.ViewModels;

public class BookingCreateVm
{
    public int SessionId { get; set; }

    // Header (інфа про фільм/сеанс)
    public string MovieTitle { get; set; } = "";
    public string? MoviePosterUrl { get; set; }
    public string? AgeLabel { get; set; }          // напр: "16+"
    public string? FormatLabel { get; set; }       // напр: "2D"
    public string? LanguageLabel { get; set; }     // напр: "SDH"
    public string? HallName { get; set; }          // напр: "Зал №1"
    public string? CinemaName { get; set; }        // напр: "Retroville ScreenX"
    public string? City { get; set; }              // напр: "Київ"
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

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
