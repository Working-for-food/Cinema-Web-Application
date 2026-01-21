using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.ViewModels.Admin.Halls;

namespace Web.Controllers.Admin;

[Area("Admin")]
public class HallsController : Controller
{
    private readonly IHallService _hallService;
    private readonly ICinemaLookupService _cinemaLookup;

    public HallsController(IHallService hallService, ICinemaLookupService cinemaLookup)
    {
        _hallService = hallService;
        _cinemaLookup = cinemaLookup;
    }

    // GET: /Admin/Halls?cinemaId=1
    public async Task<IActionResult> Index(int? cinemaId)
    {
        var cinemas = await _cinemaLookup.GetAllAsync();

        var halls = cinemaId.HasValue
            ? await _hallService.GetByCinemaAsync(cinemaId.Value)
            : await _hallService.GetAllAsync();

        var vm = new HallsIndexVm
        {
            CinemaId = cinemaId,
            Cinemas = cinemas.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = cinemaId.HasValue && cinemaId.Value == c.Id
            }).ToList(),
            Halls = halls
        };

        return View(vm);
    }

    // GET: /Admin/Halls/Create
    [HttpGet]
    public async Task<IActionResult> Create(int? cinemaId)
    {
        var cinemas = await _cinemaLookup.GetAllAsync();

        var vm = new HallEditVm
        {
            CinemaId = cinemaId ?? (cinemas.FirstOrDefault()?.Id ?? 0),
            Cinemas = cinemas.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList()
        };

        return View("Edit", vm);
    }

    // POST: /Admin/Halls/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HallEditVm vm)
    {
        if (!ModelState.IsValid)
        {
            await FillCinemasAsync(vm);
            return View("Edit", vm);
        }

        try
        {
            await _hallService.CreateAsync(new HallEditDto
            {
                CinemaId = vm.CinemaId,
                Name = vm.Name
            });

            return RedirectToAction(nameof(Index), new { cinemaId = vm.CinemaId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await FillCinemasAsync(vm);
            return View("Edit", vm);
        }
    }

    // GET: /Admin/Halls/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _hallService.GetForEditAsync(id);
        if (dto == null) return NotFound();

        var vm = new HallEditVm
        {
            Id = dto.Id,
            CinemaId = dto.CinemaId,
            Name = dto.Name
        };

        await FillCinemasAsync(vm);
        return View(vm);
    }

    // POST: /Admin/Halls/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(HallEditVm vm)
    {
        if (!ModelState.IsValid)
        {
            await FillCinemasAsync(vm);
            return View(vm);
        }

        try
        {
            await _hallService.UpdateAsync(new HallEditDto
            {
                Id = vm.Id,
                CinemaId = vm.CinemaId,
                Name = vm.Name
            });

            return RedirectToAction(nameof(Index), new { cinemaId = vm.CinemaId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await FillCinemasAsync(vm);
            return View(vm);
        }
    }

    // POST: /Admin/Halls/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? cinemaId)
    {
        await _hallService.DeleteAsync(id);
        return RedirectToAction(nameof(Index), new { cinemaId });
    }

    // GET: /Admin/Halls/Seats/5
    [HttpGet]
    public async Task<IActionResult> Seats(int id)
    {
        var already = await _hallService.SeatsAlreadyGeneratedAsync(id);
        var seats = already ? await _hallService.GetSeatsAsync(id) : new List<SeatDto>();

        ViewBag.HallId = id;
        ViewBag.AlreadyGenerated = already;

        return View(seats);
    }

    // POST: /Admin/Halls/GenerateSeats
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateSeats(int hallId, int rows, int seatsPerRow, bool allowRegenerate)
    {
        try
        {
            await _hallService.GenerateSeatsAsync(hallId, rows, seatsPerRow, allowRegenerate);
            return RedirectToAction(nameof(Seats), new { id = hallId });
        }
        catch (Exception ex)
        {
            TempData["SeatsError"] = ex.Message;
            return RedirectToAction(nameof(Seats), new { id = hallId });
        }
    }

    private async Task FillCinemasAsync(HallEditVm vm)
    {
        var cinemas = await _cinemaLookup.GetAllAsync();
        vm.Cinemas = cinemas.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == vm.CinemaId)).ToList();
    }
}
