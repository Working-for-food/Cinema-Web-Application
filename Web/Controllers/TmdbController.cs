using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("admin/tmdb")]
public class TmdbController : Controller
{
    private readonly ITmdbClient _tmdb;

    public TmdbController(ITmdbClient tmdb)
    {
        _tmdb = tmdb;
    }
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, CancellationToken ct)
    {
        var items = await _tmdb.SearchMovieAsync(query, page: 1, ct: ct);

        // Return only what the UI needs for selection
        var result = items.Select(x => new
        {
            tmdbId = x.Id,
            title = x.Title,
            releaseDate = x.ReleaseDate,
            posterPath = x.PosterPath
        });

        return Ok(result);
    }
    [HttpGet("test-import")]
    public async Task<IActionResult> TestImport([FromQuery] int tmdbId, CancellationToken ct)
    {
        var details = await _tmdb.GetMovieDetailsAsync(tmdbId, ct);
        var credits = await _tmdb.GetCreditsAsync(tmdbId, ct);
        var videos = await _tmdb.GetVideosAsync(tmdbId, ct);

        var directors = credits.Crew
            .Where(c => c.Job == "Director")
            .Select(d => new { d.Id, d.Name, d.ProfilePath })
            .ToList();

        var castTop10 = credits.Cast
            .OrderBy(c => c.Order)
            .Take(10)
            .Select(a => new { a.Id, a.Name, a.Character, a.Order, a.ProfilePath })
            .ToList();

        var trailer = videos.Results
            .FirstOrDefault(v => v.Site == "YouTube" && v.Type == "Trailer" && !string.IsNullOrWhiteSpace(v.Key));

        var trailerUrl = trailer is null ? null : $"https://www.youtube.com/watch?v={trailer.Key}";

        return Ok(new
        {
            details,
            directors,
            castTop10,
            trailerUrl
        });
    }
}
