namespace Application.DTOs;

public class GenerateSeatsDto
{
    public int HallId { get; set; }
    public string HallName { get; set; } = "";

    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }

    public bool AlreadyGenerated { get; set; }
    public bool CanEditSeats { get; set; }
    public string? LockReason { get; set; }

    public List<RowSeatsDto> RowConfigs { get; set; } = new();
    public List<SeatDto> Seats { get; set; } = new();
}
