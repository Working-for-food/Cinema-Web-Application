using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs;
public sealed class TmdbVideosResponse
{
    [JsonPropertyName("results")]
    public List<TmdbVideo> Results { get; set; } = new();
}
public sealed class TmdbVideo
{
    [JsonPropertyName("site")]
    public string? Site { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("official")]
    public bool? Official { get; set; }
}
