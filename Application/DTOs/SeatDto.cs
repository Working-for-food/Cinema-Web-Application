namespace Application.DTOs;

using Infrastructure.Entities;

public class SeatDto
{
    public int Id { get; set; }
    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }
    public SeatCategory Category { get; set; } = SeatCategory.Standard;
}
