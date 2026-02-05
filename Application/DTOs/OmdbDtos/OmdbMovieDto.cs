using System.Text.Json.Serialization;

namespace Application.DTOs.OmdbDtos;

public sealed class OmdbMovieResponse
{
    [JsonPropertyName("Response")]
    public string? Response { get; set; }  // "True"/"False"

    [JsonPropertyName("Ratings")]
    public List<OmdbRating> Ratings { get; set; } = new();
}

public sealed class OmdbRating
{
    public string? Source { get; set; }   // "Metacritic"
    public string? Value { get; set; }    // "74/100"
}
