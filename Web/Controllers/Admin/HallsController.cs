using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Services;
using Web.ViewModels.Halls;

namespace Web.Controllers.Admin;

[Route("Admin/[controller]/[action]")]
public class HallsController : Controller
{
    private readonly IHallService _hallService;
    private readonly ICinemaRepository _cinemas;

    public HallsController(IHallService hallService, ICinemaRepository cinemas)
    {
        _hallService = hallService;
        _cinemas = cinemas;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _hallService.GetAllAsync();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await FillCinemasAsync();
        return View("Edit", new HallEditVm());
    }

    [HttpPost]
    public async Task<IActionResult> Create(HallEditVm vm)
    {
        if (!ModelState.IsValid)
        {
            await FillCinemasAsync();
            return View("Edit", vm);
        }

        try
        {
            await _hallService.CreateAsync(vm);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await FillCinemasAsync();
            return View("Edit", vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _hallService.GetForEditAsync(id);
        if (vm == null) return NotFound();

        await FillCinemasAsync();
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(HallEditVm vm)
    {
        if (!ModelState.IsValid)
        {
            await FillCinemasAsync();
            return View(vm);
        }

        try
        {
            await _hallService.UpdateAsync(vm);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await FillCinemasAsync();
            return View(vm);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _hallService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // Seats screen
    [HttpGet]
    public async Task<IActionResult> GenerateSeats(int id)
    {
        var hall = await _hallService.GetForEditAsync(id);
        if (hall == null) return NotFound();

        var vm = new GenerateSeatsVm
        {
            HallId = id,
            HallName = hall.Name,
            AlreadyGenerated = await _hallService.SeatsAlreadyGeneratedAsync(id),
            Seats = await _hallService.GetSeatsAsync(id)
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> GenerateSeats(GenerateSeatsVm vm, bool allowRegenerate = false)
    {
        if (!ModelState.IsValid)
        {
            vm.AlreadyGenerated = await _hallService.SeatsAlreadyGeneratedAsync(vm.HallId);
            vm.Seats = await _hallService.GetSeatsAsync(vm.HallId);
            return View(vm);
        }

        try
        {
            vm.Seats = await _hallService.GenerateSeatsAsync(vm.HallId, vm.Rows, vm.SeatsPerRow, allowRegenerate);
            vm.AlreadyGenerated = true;
            return View(vm);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            vm.AlreadyGenerated = await _hallService.SeatsAlreadyGeneratedAsync(vm.HallId);
            vm.Seats = await _hallService.GetSeatsAsync(vm.HallId);
            return View(vm);
        }
    }

    private async Task FillCinemasAsync()
    {
        var cinemas = await _cinemas.GetAllAsync();
        ViewBag.Cinemas = cinemas.Select(c =>
            new SelectListItem(c.Name, c.Id.ToString())
        ).ToList();
    }
}
