using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs.TmdbDtos;

public sealed class TmdbMovieReleaseDatesResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("results")]
    public List<TmdbReleaseDatesByCountry> Results { get; set; } = new();
}

public sealed class TmdbReleaseDatesByCountry
{
    [JsonPropertyName("iso_3166_1")]
    public string Iso3166_1 { get; set; } = "";

    [JsonPropertyName("release_dates")]
    public List<TmdbReleaseDateItem> ReleaseDates { get; set; } = new();
}

public sealed class TmdbReleaseDateItem
{
    [JsonPropertyName("certification")]
    public string Certification { get; set; } = "";
}