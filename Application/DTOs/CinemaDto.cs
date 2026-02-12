namespace Application.DTOs;

public class CinemaDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string City { get; set; } = null!;
    public string? ImageUrl { get; set; }
}