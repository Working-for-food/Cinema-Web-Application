namespace Application.DTOs;

using Infrastructure.Entities;

public class RowCategoryDto
{
    public int RowNumber { get; set; }
    public SeatCategory Category { get; set; } = SeatCategory.Standard;
}