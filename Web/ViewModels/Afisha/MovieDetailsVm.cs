namespace Web.ViewModels.Movies;

public class MovieDetailsVm
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
    public IReadOnlyList<string> Directors { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Countries { get; set; } = Array.Empty<string>();
}
