namespace Web.ViewModels.Movies.Details;

public sealed class MovieDetailsVm
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DateOnly? ReleaseDate { get; set; }
    public string? OriginalName { get; set; }
    public string? Description { get; set; }
    public string? Language { get; set; }
    public int? Duration { get; set; }
    public string? TrailerUrl { get; set; }
    public decimal? Rating { get; set; }
    public string PosterUrl { get; set; } = "";
    public string? Metacritic { get; set; }
    public string? RottenTomatoes { get; set; }
    public string? Imdb { get; set; }
    public IReadOnlyList<string> Directors { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Countries { get; set; } = Array.Empty<string>();
    public DateOnly? SelectedDate { get; set; }
    public IReadOnlyList<string> Actors { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Genres { get; set; } = Array.Empty<string>();
    public int? AgeRating { get; set; }
    public IReadOnlyList<MovieCinemaScheduleVm> Schedule { get; set; } = Array.Empty<MovieCinemaScheduleVm>();
}