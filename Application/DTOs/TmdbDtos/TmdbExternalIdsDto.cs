using System.Text.Json.Serialization;

namespace Application.DTOs.TmdbDtos;

public sealed class TmdbExternalIdsResponse
{
    [JsonPropertyName("imdb_id")]
    public string? ImdbId { get; set; }
}
