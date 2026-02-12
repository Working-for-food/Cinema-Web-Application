namespace Application.DTOs.OmdbDtos;

public sealed class ExternalRatingsDto
{
    public string? Imdb { get; set; }            // "7.8/10"
    public string? RottenTomatoes { get; set; }  // "82%"
    public string? Metacritic { get; set; }      // "74/100"
}
