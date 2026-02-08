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

    public OmdbRatingsService(
        HttpClient http,
        IOptions<OmdbOptions> opt,
        IMemoryCache cache)
    {
        _http = http;
        _opt = opt.Value;
        _cache = cache;
    }

    public async Task<ExternalRatingsDto> GetRatingsAsync(
        string imdbId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(imdbId))
            return new ExternalRatingsDto();

        var cacheKey = $"omdb:imdb:{imdbId}";
        if (_cache.TryGetValue(cacheKey, out ExternalRatingsDto cached))
            return cached;

        var url = $"?apikey={_opt.ApiKey}&i={Uri.EscapeDataString(imdbId)}";

        OmdbMovieResponse? response;
        try
        {
            response = await _http.GetFromJsonAsync<OmdbMovieResponse>(url, JsonOpts, ct);
        }
        catch
        {
            return new ExternalRatingsDto();
        }

        if (response?.Response?.Equals("True", StringComparison.OrdinalIgnoreCase) != true)
            return new ExternalRatingsDto();

        string? Get(string source) =>
            response.Ratings
                .FirstOrDefault(r =>
                    r.Source?.Equals(source, StringComparison.OrdinalIgnoreCase) == true)
                ?.Value;

        var dto = new ExternalRatingsDto
        {
            Imdb = Get("Internet Movie Database"),
            RottenTomatoes = Get("Rotten Tomatoes"),
            Metacritic = Get("Metacritic")
        };

        _cache.Set(cacheKey, dto, TimeSpan.FromHours(12));
        return dto;
    }

    public async Task<ExternalRatingsDto> GetRatingsByTitleAsync(
        string title,
        int? year,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title))
            return new ExternalRatingsDto();

        var cacheKey = $"omdb:title:{title}:{year}";
        if (_cache.TryGetValue(cacheKey, out ExternalRatingsDto cached))
            return cached;

        var encodedTitle = Uri.EscapeDataString(title);
        var yearPart = year.HasValue ? $"&y={year.Value}" : string.Empty;

        var url = $"?apikey={_opt.ApiKey}&t={encodedTitle}{yearPart}";

        OmdbMovieResponse? response;
        try
        {
            response = await _http.GetFromJsonAsync<OmdbMovieResponse>(url, JsonOpts, ct);
        }
        catch
        {
            return new ExternalRatingsDto();
        }

        if (response?.Response?.Equals("True", StringComparison.OrdinalIgnoreCase) != true)
            return new ExternalRatingsDto();

        string? Get(string source) =>
            response.Ratings
                .FirstOrDefault(r =>
                    r.Source?.Equals(source, StringComparison.OrdinalIgnoreCase) == true)
                ?.Value;

        var dto = new ExternalRatingsDto
        {
            Imdb = Get("Internet Movie Database"),
            RottenTomatoes = Get("Rotten Tomatoes"),
            Metacritic = Get("Metacritic")
        };

        _cache.Set(cacheKey, dto, TimeSpan.FromHours(12));
        return dto;
    }
}
