using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web.ViewModels.Admin.Halls;

namespace Web.Controllers.Admin;

[Area("Admin")]
public class HallsController : Controller
{
    private readonly IHallService _hallService;
    private readonly CinemaDbContext _db;

    private const string IndexViewPath = "~/Views/Admin/Halls/Index.cshtml";
    private const string EditViewPath = "~/Views/Admin/Halls/Edit.cshtml";
    private const string SeatsViewPath = "~/Views/Admin/Halls/Seats.cshtml";

    public HallsController(IHallService hallService, CinemaDbContext db)
    {
        _hallService = hallService;
        _db = db;
    }

    // GET: /Admin/Halls?cinemaId=1
    public async Task<IActionResult> Index(int? cinemaId)
    {
        var cinemas = await _db.Cinemas
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

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

        return View(IndexViewPath, vm);
    }

    // GET: /Admin/Halls/Create
    [HttpGet]
    public async Task<IActionResult> Create(int? cinemaId)
    {
        var cinemas = await _db.Cinemas
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        var vm = new HallEditVm
        {
            CinemaId = cinemaId ?? (cinemas.FirstOrDefault()?.Id ?? 0),
            Cinemas = cinemas.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList()
        };

        return View(EditViewPath, vm);
    }

    // POST: /Admin/Halls/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HallEditVm vm)
    {
        if (!ModelState.IsValid)
        {
            await FillCinemasAsync(vm);
            return View(EditViewPath, vm);
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
            return View(EditViewPath, vm);
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
        return View(EditViewPath, vm);
    }

    // POST: /Admin/Halls/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(HallEditVm vm)
    {
        if (!ModelState.IsValid)
        {
            await FillCinemasAsync(vm);
            return View(EditViewPath, vm);
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
            return View(EditViewPath, vm);
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
    public async Task<IActionResult> Seats(int id, int? rows, int? seatsPerRow)
    {
        var dto = await _hallService.GetSeatingAsync(id, rows, seatsPerRow);
        if (dto == null) return NotFound();

        ViewBag.SeatsError = TempData["SeatsError"] as string;
        return View(SeatsViewPath, dto);
    }

    // POST: /Admin/Halls/ConfigureSeats
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfigureSeats(int hallId, int rows, int seatsPerRow)
    {
        return RedirectToAction(nameof(Seats), new { id = hallId, rows, seatsPerRow });
    }

    // POST: /Admin/Halls/GenerateSeats
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateSeats(int hallId, bool allowRegenerate, List<RowSeatsDto> rowConfigs)
    {
        try
        {
            await _hallService.GenerateSeatsByConfigAsync(hallId, rowConfigs, allowRegenerate);
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
        var cinemas = await _db.Cinemas
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        vm.Cinemas = cinemas
            .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == vm.CinemaId))
            .ToList();
    }
}
