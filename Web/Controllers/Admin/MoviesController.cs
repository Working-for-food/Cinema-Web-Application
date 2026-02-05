using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web.ViewModels.Admin;

namespace Web.Controllers.Admin;

[Route("admin/movies")]
public class MoviesController : Controller
{
    private const string ViewsRoot = "~/Views/Admin/Movies";
    private readonly IMovieService _service;
    private readonly ITmdbClient _tmdb;
    private readonly IImportMovieFromTmdb _import;
    private readonly IMetacriticService _metacritic;

    public MoviesController(IMovieService service, ITmdbClient tmdb, IImportMovieFromTmdb import, IMetacriticService metacritic)
    {
        _service = service;
        _tmdb = tmdb;
        _import = import;
        _metacritic = metacritic;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, string? sortBy = "title", int page = 1, CancellationToken ct = default)
    {
        var (movies, totalCount) = await _service.GetMoviesAsync(search, sortBy, page, ct);

        var vm = new MovieIndexVm
        {
            Movies = movies,
            Page = page < 1 ? 1 : page,
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / 10.0)),
            SearchTerm = search,
            SortBy = sortBy
        };

        return View($"{ViewsRoot}/Index.cshtml", vm);
    }

    [HttpGet("details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var movie = await _service.GetMovieDetailsAsync(id, ct);
        if (movie == null) return NotFound();

        var metacriticDisplay = "—";

        if (movie.TmdbId.HasValue)
        {
            var ext = await _tmdb.GetExternalIdsAsync(movie.TmdbId.Value, ct);
            if (!string.IsNullOrWhiteSpace(ext.ImdbId))
            {
                var meta = await _metacritic.GetMetacriticAsync(ext.ImdbId, ct);
                if (!string.IsNullOrWhiteSpace(meta))
                    metacriticDisplay = meta!;
            }
        }

        var vm = new MovieDetailsVm
        {
            Movie = movie,
            Metacritic = metacriticDisplay
        };

        return View($"{ViewsRoot}/Details.cshtml", vm);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var vm = new MovieEditVm();
        await FillLookupsAsync(vm, ct);
        return View($"{ViewsRoot}/Create.cshtml", vm);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(MovieEditVm model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await FillLookupsAsync(model, ct);
            return View($"{ViewsRoot}/Create.cshtml", model);
        }

        var dto = MapToDto(model);
        var (ok, error) = await _service.CreateAsync(dto, ct);

        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            await FillLookupsAsync(model, ct);
            return View($"{ViewsRoot}/Create.cshtml", model);
        }

        TempData["Success"] = "Фільм створено.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var dto = await _service.GetMovieForEditAsync(id, ct);
        if (dto == null) return NotFound();

        var vm = new MovieEditVm
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Duration = dto.Duration,
            ReleaseDate = dto.ReleaseDate,
            PosterPath = dto.PosterPath,
            BackdropPath = dto.BackdropPath,
            OriginalName = dto.OriginalName,
            Language = dto.Language,
            TrailerUrl = dto.TrailerUrl,
            SelectedGenreIds = dto.GenreIds,
            SelectedActorIds = dto.ActorIds,
            SelectedDirectorIds = dto.DirectorIds,
            SelectedCountryCodes = dto.CountryCodes
        };

        await FillLookupsAsync(vm, ct);
        return View($"{ViewsRoot}/Edit.cshtml", vm);
    }

    [HttpPost("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, MovieEditVm model, CancellationToken ct)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            await FillLookupsAsync(model, ct);
            return View($"{ViewsRoot}/Edit.cshtml", model);
        }

        var dto = MapToDto(model);
        var (ok, error) = await _service.UpdateAsync(dto, ct);

        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            await FillLookupsAsync(model, ct);
            return View($"{ViewsRoot}/Edit.cshtml", model);
        }

        TempData["Success"] = "Фільм оновлено.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var (ok, error) = await _service.DeleteAsync(id, ct);
        TempData[ok ? "Success" : "Error"] = ok ? "Фільм видалено." : error;
        return RedirectToAction(nameof(Index));
    }
    [HttpGet("tmdb/add")]
    public IActionResult AddFromTmdb()
    {
        return View($"{ViewsRoot}/AddFromTmdb.cshtml");
    }
    [HttpGet("tmdb/search")]
    public async Task<IActionResult> SearchTmdb([FromQuery] string query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(Array.Empty<object>());

        var items = await _tmdb.SearchMovieAsync(query, page: 1, ct);

        var result = items
            .Select(x =>
            {
                DateTime? parsed = null;
                if (DateTime.TryParse(x.ReleaseDate, out var dt))
                    parsed = dt;

                return new
                {
                    tmdbId = x.Id,
                    title = x.Title,
                    releaseDate = x.ReleaseDate,
                    posterPath = x.PosterPath,
                    releaseDateParsed = parsed
                };
            })
            .OrderByDescending(x => x.releaseDateParsed ?? DateTime.MinValue)
            .Select(x => new
            {
                x.tmdbId,
                x.title,
                x.releaseDate,
                x.posterPath
            });

        return Ok(result);
    }
    [HttpPost("tmdb/import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFromTmdb([FromForm] int tmdbId, CancellationToken ct)
    {
        var movieId = await _import.ImportAsync(tmdbId, ct);
        return RedirectToAction(nameof(Edit), new { id = movieId });
    }
    private async Task FillLookupsAsync(MovieEditVm vm, CancellationToken ct)
    {
        var genres = await _service.GetGenresAsync(ct);
        vm.GenreList = new MultiSelectList(genres, "Id", "Name", vm.SelectedGenreIds);

        var countries = await _service.GetCountriesAsync(ct);
        vm.CountryMultiList = new MultiSelectList(countries, "Code", "Name", vm.SelectedCountryCodes);

        var directors = await _service.GetDirectorsAsync(ct);
        vm.DirectorMultiList = new MultiSelectList(directors, "Id", "FullName", vm.SelectedDirectorIds);

        var actors = await _service.GetActorsAsync(ct);
        vm.ActorList = new MultiSelectList(actors, "Id", "FullName", vm.SelectedActorIds);
    }
    

    private static MovieFormDto MapToDto(MovieEditVm vm) => new()
    {
        Id = vm.Id,
        Title = vm.Title,
        Description = vm.Description,
        Duration = vm.Duration,
        ReleaseDate = vm.ReleaseDate,
        PosterPath = vm.PosterPath,
        BackdropPath = vm.BackdropPath,
        OriginalName = vm.OriginalName,
        Language = vm.Language,
        TrailerUrl = vm.TrailerUrl,
        GenreIds = vm.SelectedGenreIds,
        ActorIds = vm.SelectedActorIds,
        CountryCodes = vm.SelectedCountryCodes,
        DirectorIds = vm.SelectedDirectorIds
    };
}
