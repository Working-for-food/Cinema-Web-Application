using Application.Interfaces;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class CinemasController : Controller
{
    private readonly ICinemaService _cinemaService;

    public CinemasController(ICinemaService cinemaService)
    {
        _cinemaService = cinemaService;
    }

    // GET: /Cinemas
    public async Task<IActionResult> Index()
    {
        var cinemas = await _cinemaService.GetAllAsync();
        return View(cinemas);
    }

    // GET: /Cinemas/Create
    public IActionResult Create()
    {
        return View(new CinemaEditVm());
    }

    // POST: /Cinemas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CinemaEditVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        await _cinemaService.CreateAsync(vm);
        return RedirectToAction(nameof(Index));
    }

    // GET: /Cinemas/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _cinemaService.GetForEditAsync(id);
        if (vm == null) return NotFound();

        return View(vm);
    }

    // POST: /Cinemas/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CinemaEditVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var updated = await _cinemaService.UpdateAsync(vm);
        if (!updated) return NotFound();

        return RedirectToAction(nameof(Index));
    }

    // GET: /Cinemas/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _cinemaService.GetForEditAsync(id);
        if (vm == null) return NotFound();

        return View(vm);
    }

    // POST: /Cinemas/DeleteConfirmed/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _cinemaService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
