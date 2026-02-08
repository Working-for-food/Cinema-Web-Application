using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Movies.Details;

namespace Web.Controllers;

[Route("movies")]
public class MoviesController : Controller
{
    private readonly IMoviePublicService _moviePublicService;
    private readonly IExternalRatingsService _externalRatingsService;
    private readonly ITmdbClient _tmdb;

    public MoviesController(
        IMoviePublicService moviePublicService,
        IExternalRatingsService externalRatingsService,
        ITmdbClient tmdb)
    {
        _moviePublicService = moviePublicService;
        _externalRatingsService = externalRatingsService;
        _tmdb = tmdb;
    }

    [HttpGet("{id:int}", Name = "MovieDetails")]
    public async Task<IActionResult> Details(int id, [FromQuery] DateOnly? date, CancellationToken ct)
    {
        var dto = await _moviePublicService.GetDetailsAsync(id, date, ct);
        if (dto is null) return NotFound();

        var firstDay = dto.Schedule
            .SelectMany(c => c.Days)
            .Select(d => (DateOnly?)d.Date)
            .Min();

        var selected = date ?? firstDay;

        var vm = new MovieDetailsVm
        {
            Id = dto.Id,
            Title = dto.Title,
            ReleaseDate = dto.ReleaseDate,
            OriginalName = dto.OriginalName,
            Description = dto.Description,
            Language = dto.Language,
            Duration = dto.Duration,
            TrailerUrl = dto.TrailerUrl,
            Rating = dto.Rating,
            PosterUrl = NormalizePosterUrl(dto.PosterPath),

            TmdbId = dto.TmdbId,
            AgeRating = dto.AgeRating,

            Directors = dto.Directors.Select(d => d.Name).ToList(),
            Countries = dto.Countries.ToList(),
            Genres = dto.Genres.ToList(),
            Actors = dto.Actors.Select(a => a.Name).ToList(),

            SelectedDate = selected,
            Schedule = dto.Schedule.Select(c => new MovieCinemaScheduleVm
            {
                CinemaId = c.CinemaId,
                CinemaName = c.CinemaName,
                Days = c.Days.Select(d => new MovieScheduleDayVm
                {
                    Date = d.Date,
                    Sessions = d.Slots.Select(s => new MovieSessionVm
                    {
                        Id = s.Id,
                        StartTime = s.Start,
                        EndTime = s.End,
                        HallName = s.HallName,
                        Presentation = s.Presentation
                    }).ToList()
                }).ToList()
            }).ToList()
        };

        string? imdbId = null;
        if (vm.TmdbId.HasValue)
        {
            var ext = await _tmdb.GetExternalIdsAsync(vm.TmdbId.Value, ct);
            imdbId = ext.ImdbId;
        }

        var ratings = !string.IsNullOrWhiteSpace(imdbId)
            ? await _externalRatingsService.GetRatingsAsync(imdbId!, ct)
            : await _externalRatingsService.GetRatingsByTitleAsync(vm.OriginalName ?? vm.Title, vm.ReleaseDate?.Year, ct);

        vm.Imdb = ratings.Imdb;
        vm.RottenTomatoes = ratings.RottenTomatoes;
        vm.Metacritic = ratings.Metacritic;

        return View(vm);
    }

    private static string NormalizePosterUrl(string? poster)
    {
        if (string.IsNullOrWhiteSpace(poster))
            return "/images/no-poster.png";

        if (poster.StartsWith("http://") || poster.StartsWith("https://"))
            return poster;

        if (poster.StartsWith("/"))
            return "https://image.tmdb.org/t/p/w500" + poster;

        return "/" + poster.TrimStart('/');
    }
}
