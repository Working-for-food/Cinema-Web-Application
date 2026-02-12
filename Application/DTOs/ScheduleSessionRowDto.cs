using Infrastructure.Entities;

namespace Application.DTOs;

public sealed class ScheduleSessionRowDto
{
    public int SessionId { get; init; }
    public DateTime StartTime { get; init; }
    public int MovieId { get; init; }
    public string MovieTitle { get; init; } = "";
    public string? PosterPath { get; init; }
    public short? AgeRating { get; init; }
    public PresentationType PresentationType { get; init; }
}