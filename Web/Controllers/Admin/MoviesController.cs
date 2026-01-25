using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.ViewModels.Admin;

namespace Web.Controllers.Admin;

[Route("admin/movies")]
public class MoviesController : Controller
{
    private const string ViewsRoot = "~/Views/Admin/Movies";
    private readonly MovieService _service;

    public MoviesController(MovieService service) => _service = service;

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
        return View($"{ViewsRoot}/Details.cshtml", movie);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var genres = await _service.GetGenresAsync(ct);
        var vm = new MovieEditVm
        {
            GenreList = new MultiSelectList(genres, "Id", "Name")
        };

        return View($"{ViewsRoot}/Create.cshtml", vm);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MovieEditVm model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await FillGenresAsync(model, ct);
            return View($"{ViewsRoot}/Create.cshtml", model);
        }

        try
        {
            var dto = MapToDto(model);
            await _service.CreateMovieAsync(dto, ct);
            TempData["Success"] = "Фільм створено.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await FillGenresAsync(model, ct);
            return View($"{ViewsRoot}/Create.cshtml", model);
        }
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        try
        {
            var dto = await _service.GetMovieForEditAsync(id, ct);
            var genres = await _service.GetGenresAsync(ct);

            var vm = new MovieEditVm
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                Duration = dto.Duration,
                ReleaseDate = dto.ReleaseDate,
                ProductionCountryCode = dto.ProductionCountryCode,
                SelectedGenreIds = dto.GenreIds,
                GenreList = new MultiSelectList(genres, "Id", "Name", dto.GenreIds)
            };

            return View($"{ViewsRoot}/Edit.cshtml", vm);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MovieEditVm model, CancellationToken ct)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            await FillGenresAsync(model, ct);
            return View($"{ViewsRoot}/Edit.cshtml", model);
        }

        try
        {
            var dto = MapToDto(model);
            await _service.UpdateMovieAsync(dto, ct);
            TempData["Success"] = "Фільм оновлено.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await FillGenresAsync(model, ct);
            return View($"{ViewsRoot}/Edit.cshtml", model);
        }
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteMovieAsync(id, ct);
            TempData["Success"] = "Фільм видалено.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private static MovieFormDto MapToDto(MovieEditVm vm) => new()
    {
        Id = vm.Id,
        Title = vm.Title,
        Description = vm.Description,
        Duration = vm.Duration,
        ReleaseDate = vm.ReleaseDate,
        ProductionCountryCode = vm.ProductionCountryCode,
        GenreIds = vm.SelectedGenreIds
    };

    private async Task FillGenresAsync(MovieEditVm vm, CancellationToken ct)
    {
        var genres = await _service.GetGenresAsync(ct);
        vm.GenreList = new MultiSelectList(genres, "Id", "Name", vm.SelectedGenreIds);
    }
}
