namespace Application.DTOs;

public class HallListDto
{
    public int HallId { get; set; }
    public string CinemaName { get; set; } = "";
    public string HallName { get; set; } = "";
    public int SeatsCount { get; set; }
}
