using Application.DTOs.OmdbDtos;
using Application.Interfaces;
using Application.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.Services;

public sealed class OmdbRatingsService : IExternalRatingsService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private readonly OmdbOptions _opt;
    private readonly IMemoryCache _cache;

    public OmdbRatingsService(HttpClient http, IOptions<OmdbOptions> opt, IMemoryCache cache)
    {
        _http = http;
        _opt = opt.Value;
        _cache = cache;
    }

    public async Task<ExternalRatingsDto> GetRatingsAsync(string imdbId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(imdbId))
            return new ExternalRatingsDto();

        var cacheKey = $"omdb-ratings:{imdbId.Trim()}";
        if (_cache.TryGetValue(cacheKey, out ExternalRatingsDto? cached) && cached is not null)
            return cached;

        // 1) Запрос в OMDb
        var url = $"?apikey={Uri.EscapeDataString(_opt.ApiKey)}&i={Uri.EscapeDataString(imdbId.Trim())}";
        OmdbMovieResponse? resp = null;

        try
        {
            resp = await _http.GetFromJsonAsync<OmdbMovieResponse>(url, JsonOpts, ct);
        }
        catch
        {
            return new ExternalRatingsDto(); 
        }

        if (resp?.Response?.Equals("True", StringComparison.OrdinalIgnoreCase) != true)
            return new ExternalRatingsDto();

        string? GetValue(string source) =>
            resp.Ratings
                .FirstOrDefault(r => r.Source?.Equals(source, StringComparison.OrdinalIgnoreCase) == true)
                ?.Value
                ?.Trim();

        var dto = new ExternalRatingsDto
        {
            Imdb = Normalize(GetValue("Internet Movie Database")),
            RottenTomatoes = Normalize(GetValue("Rotten Tomatoes")),
            Metacritic = Normalize(GetValue("Metacritic"))
        };

        _cache.Set(cacheKey, dto, TimeSpan.FromHours(12));
        return dto;
    }

    private static string? Normalize(string? v)
        => string.IsNullOrWhiteSpace(v) || v.Equals("N/A", StringComparison.OrdinalIgnoreCase) ? null : v;
}
