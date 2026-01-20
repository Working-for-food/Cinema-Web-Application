using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Halls;

public class GenerateSeatsVm
{
    public int HallId { get; set; }
    public string HallName { get; set; } = "";

    [Range(1, 200)]
    public int Rows { get; set; } = 10;

    [Range(1, 300)]
    public int SeatsPerRow { get; set; } = 12;

    public bool AlreadyGenerated { get; set; }

    public List<SeatVm> Seats { get; set; } = new();
}

public class SeatVm
{
    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }
}
