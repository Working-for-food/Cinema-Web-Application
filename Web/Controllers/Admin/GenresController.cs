using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Genres;

namespace Web.Controllers.Admin;

[Route("admin/genres")]
public class GenresController : Controller
{
    private const string ViewsRoot = "~/Views/Admin/Genres";
    private readonly IGenreService _service;

    public GenresController(IGenreService service) => _service = service;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var genres = await _service.GetAllAsync(ct);
        return View($"{ViewsRoot}/Index.cshtml", genres);
    }

    [HttpGet("details/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var genre = await _service.GetByIdAsync(id, ct);
        if (genre == null) return NotFound();
        return View($"{ViewsRoot}/Details.cshtml", genre);
    }

    [HttpGet("create")]
    public IActionResult Create() => View($"{ViewsRoot}/Create.cshtml", new GenreEditVm());

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GenreEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View($"{ViewsRoot}/Create.cshtml", vm);

        var (ok, error) = await _service.CreateAsync(vm.Name, ct);
        if (!ok)
        {
            ModelState.AddModelError(nameof(vm.Name), error!);
            return View($"{ViewsRoot}/Create.cshtml", vm);
        }

        TempData["Success"] = "Жанр створено.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var genre = await _service.GetByIdAsync(id, ct);
        if (genre == null) return NotFound();

        return View($"{ViewsRoot}/Edit.cshtml", new GenreEditVm { Id = genre.Id, Name = genre.Name });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GenreEditVm vm, CancellationToken ct)
    {
        vm.Id = id;

        if (!ModelState.IsValid)
            return View($"{ViewsRoot}/Edit.cshtml", vm);

        var (ok, error) = await _service.UpdateAsync(vm.Id, vm.Name, ct);
        if (!ok)
        {
            ModelState.AddModelError(nameof(vm.Name), error!);
            return View($"{ViewsRoot}/Edit.cshtml", vm);
        }

        TempData["Success"] = "Жанр оновлено.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var genre = await _service.GetByIdAsync(id, ct);
        if (genre == null) return NotFound();

        return View($"{ViewsRoot}/Delete.cshtml", genre);
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var (ok, error) = await _service.DeleteAsync(id, ct);
        if (!ok)
        {
            TempData["Error"] = error ?? "Неможливо видалити жанр.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Success"] = "Жанр видалено.";
        return RedirectToAction(nameof(Index));
    }
}
