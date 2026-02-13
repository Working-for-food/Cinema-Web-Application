namespace Application.DTOs.Movies;

public sealed class MovieDetailsDtoRelatedItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? PosterPath { get; set; }
}
