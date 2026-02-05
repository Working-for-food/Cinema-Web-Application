using Application.DTOs.OmdbDtos;
using Application.Interfaces;
using Application.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.Services;

public sealed class MetacriticService : IMetacriticService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private readonly OmdbOptions _opt;
    private readonly IMemoryCache _cache;

    public MetacriticService(HttpClient http, IOptions<OmdbOptions> opt, IMemoryCache cache)
    {
        _http = http;
        _opt = opt.Value;
        _cache = cache;
    }

    public async Task<string?> GetMetacriticAsync(string imdbId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(imdbId)) return null;

        var cacheKey = $"omdb-metacritic:{imdbId.Trim()}";
        if (_cache.TryGetValue(cacheKey, out string? cached))
            return cached;

        var url = $"?apikey={Uri.EscapeDataString(_opt.ApiKey)}&i={Uri.EscapeDataString(imdbId.Trim())}";

        OmdbMovieResponse? resp = null;
        try
        {
            resp = await _http.GetFromJsonAsync<OmdbMovieResponse>(url, JsonOpts, ct);
        }
        catch
        {
            return null;
        }

        if (resp?.Response?.Equals("True", StringComparison.OrdinalIgnoreCase) != true)
            return null;

        var meta = resp.Ratings
            .FirstOrDefault(r => r.Source?.Equals("Metacritic", StringComparison.OrdinalIgnoreCase) == true)
            ?.Value;

        _cache.Set(cacheKey, meta, TimeSpan.FromHours(12));
        return meta;
    }
}
