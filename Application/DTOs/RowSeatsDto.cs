namespace Application.DTOs;

using Infrastructure.Entities;

public class RowSeatsDto
{
    public int RowNumber { get; set; }
    public int SeatsCount { get; set; }
    public SeatCategory Category { get; set; } = SeatCategory.Standard;
}
