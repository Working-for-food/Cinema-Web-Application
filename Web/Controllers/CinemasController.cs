using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class CinemasController : Controller
{
    private readonly ICinemaService _cinemaService;

    public CinemasController(ICinemaService cinemaService)
    {
        _cinemaService = cinemaService;
    }

    // GET: /Cinemas?city=&search=&sort=
    public async Task<IActionResult> Index(string? city, string? search, string? sort)
    {
        ViewBag.City = city ?? "";
        ViewBag.Search = search ?? "";
        ViewBag.Sort = sort ?? "";
        ViewBag.Cities = await _cinemaService.GetCitiesAsync();

        var cinemas = await _cinemaService.GetAllAsync(city, search, sort);
        return View(cinemas);
    }

    // GET: /Cinemas/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var dto = await _cinemaService.GetDetailsAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    // GET: /Cinemas/Create
    public IActionResult Create()
        => View(new CinemaEditDto());

    // POST: /Cinemas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CinemaEditDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        await _cinemaService.CreateAsync(dto);
        TempData["Success"] = "Cinema created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Cinemas/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _cinemaService.GetForEditAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    // POST: /Cinemas/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CinemaEditDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _cinemaService.UpdateAsync(dto);
            TempData["Success"] = "Cinema updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }

    // GET: /Cinemas/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var dto = await _cinemaService.GetForEditAsync(id);
        if (dto == null) return NotFound();

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (id <= 0) return BadRequest();

        try
        {
            await _cinemaService.DeleteAsync(id);
            TempData["Success"] = "Cinema deleted successfully.";
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
