using Application.DTOs;
using Application.Interfaces;
using Application.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
namespace Application.Services;
public sealed class TmdbClient : ITmdbClient
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    private readonly HttpClient _http;
    private readonly TmdbOptions _opt;

    public TmdbClient(HttpClient http, IOptions<TmdbOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
    }
    public async Task<IReadOnlyList<TmdbMovieSearchItem>> SearchMovieAsync(
    string query,
    int page = 1,
    CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<TmdbMovieSearchItem>();

        var url = $"search/movie{BuildQuery(new Dictionary<string, string?>
        {
            ["api_key"] = _opt.ApiKey,
            ["language"] = _opt.Language,
            ["query"] = query.Trim(),
            ["page"] = page.ToString(),
            ["include_adult"] = "false"
        })}";

        var resp = await _http.GetFromJsonAsync<TmdbSearchMovieResponse>(url, JsonOpts, ct);

        return (IReadOnlyList<TmdbMovieSearchItem>?)resp?.Results ?? Array.Empty<TmdbMovieSearchItem>();
    }

    public async Task<TmdbMovieDetailsResponse> GetMovieDetailsAsync(int tmdbMovieId, CancellationToken ct = default)
    {
        var url = $"movie/{tmdbMovieId}{BuildQuery(new Dictionary<string, string?>
        {
            ["api_key"] = _opt.ApiKey,
            ["language"] = _opt.Language
        })}";

        var resp = await _http.GetFromJsonAsync<TmdbMovieDetailsResponse>(url, JsonOpts, ct);
        return resp ?? throw new InvalidOperationException($"TMDB details response was empty for id={tmdbMovieId}");
    }

    public async Task<TmdbCreditsResponse> GetCreditsAsync(int tmdbMovieId, CancellationToken ct = default)
    {
        var url = $"movie/{tmdbMovieId}/credits{BuildQuery(new Dictionary<string, string?>
        {
            ["api_key"] = _opt.ApiKey,
            ["language"] = _opt.Language
        })}";

        var resp = await _http.GetFromJsonAsync<TmdbCreditsResponse>(url, JsonOpts, ct);
        return resp ?? new TmdbCreditsResponse();
    }

    public async Task<TmdbVideosResponse> GetVideosAsync(int tmdbMovieId, CancellationToken ct = default)
    {
        // Some movies have no uk-UA videos; consider fallback in your import logic to en-US
        var url = $"movie/{tmdbMovieId}/videos{BuildQuery(new Dictionary<string, string?>
        {
            ["api_key"] = _opt.ApiKey,
            ["language"] = _opt.Language
        })}";

        var resp = await _http.GetFromJsonAsync<TmdbVideosResponse>(url, JsonOpts, ct);
        return resp ?? new TmdbVideosResponse();
    }

    private static string BuildQuery(Dictionary<string, string?> kv)
    {
        var parts = kv
            .Where(p => !string.IsNullOrWhiteSpace(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}");

        return "?" + string.Join("&", parts);
    }
}
