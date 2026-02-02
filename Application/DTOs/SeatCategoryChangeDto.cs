namespace Application.DTOs;

using Infrastructure.Entities;

public class SeatCategoryChangeDto
{
    public int SeatId { get; set; }
    public SeatCategory Category { get; set; } = SeatCategory.Standard;
}