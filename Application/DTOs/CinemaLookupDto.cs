namespace Application.DTOs;

public sealed class CinemaLookupDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string? City { get; init; }
    public string? Address { get; init; }
}