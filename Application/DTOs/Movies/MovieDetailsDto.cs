namespace Application.DTOs.Movies;

public sealed class MovieDetailsDto
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public string? OriginalName { get; init; }
    public string? Description { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public int? Duration { get; init; }

    public string? PosterPath { get; init; }
    public string? BackdropPath { get; init; }
    public string? TrailerUrl { get; init; }

    public string? Language { get; init; }
    public decimal? Rating { get; init; }
    public int? AgeRating { get; init; }

    public IReadOnlyList<string> Genres { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Countries { get; init; } = Array.Empty<string>();

    public IReadOnlyList<PersonShortDto> Directors { get; init; } = Array.Empty<PersonShortDto>();
    public IReadOnlyList<PersonShortDto> Actors { get; init; } = Array.Empty<PersonShortDto>();

    public IReadOnlyList<SessionsCinemaDto> Schedule { get; init; } = Array.Empty<SessionsCinemaDto>();
}
public sealed class PersonShortDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string? PhotoUrl { get; init; }
}

public sealed class SessionsCinemaDto
{
    public int CinemaId { get; init; }
    public string CinemaName { get; init; } = "";
    public IReadOnlyList<SessionsDayDto> Days { get; init; } = Array.Empty<SessionsDayDto>();
}
public sealed class SessionsDayDto
{
    public DateOnly Date { get; init; }
    public IReadOnlyList<SessionSlotDto> Slots { get; init; } = Array.Empty<SessionSlotDto>();
}
public sealed class SessionSlotDto
{
    public int Id { get; init; }
    public TimeOnly Start { get; init; }
    public TimeOnly End { get; init; }
    public string HallName { get; init; } = "";
    public string Presentation { get; init; } = ""; // "2D" | "3D" | "IMAX"
    public bool IsCancelled { get; init; }
}