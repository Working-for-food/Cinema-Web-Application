using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Genres;

namespace Web.Controllers.Admin;

[Area("Admin")]
public class GenresController : Controller
{
    private readonly IGenreService _service;

    public GenresController(IGenreService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        var genres = await _service.GetAllAsync();
        return View(genres);
    }

    public async Task<IActionResult> Details(int id)
    {
        var genre = await _service.GetByIdAsync(id);
        if (genre == null) return NotFound();
        return View(genre);
    }

    [HttpGet]
    public IActionResult Create() => View(new GenreEditVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GenreEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var (ok, error) = await _service.CreateAsync(vm.Name);
        if (!ok)
        {
            ModelState.AddModelError(nameof(vm.Name), error!);
            return View(vm);
        }

        TempData["Success"] = "Жанр створено.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var genre = await _service.GetByIdAsync(id);
        if (genre == null) return NotFound();

        return View(new GenreEditVm { Id = genre.Id, Name = genre.Name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(GenreEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var (ok, error) = await _service.UpdateAsync(vm.Id, vm.Name);
        if (!ok)
        {
            ModelState.AddModelError(nameof(vm.Name), error!);
            return View(vm);
        }

        TempData["Success"] = "Жанр оновлено.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var genre = await _service.GetByIdAsync(id);
        if (genre == null) return NotFound();

        return View(genre);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var (ok, error) = await _service.DeleteAsync(id);
        if (!ok)
        {
            TempData["Error"] = "Неможливо видалити: жанр використовується у фільмах.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Success"] = "Жанр видалено.";
        return RedirectToAction(nameof(Index));
    }
}
