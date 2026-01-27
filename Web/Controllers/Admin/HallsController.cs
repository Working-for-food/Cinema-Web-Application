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
    private readonly ICinemaService _cinemaService;

    private const string IndexViewPath = "~/Views/Admin/Halls/Index.cshtml";
    private const string EditViewPath = "~/Views/Admin/Halls/Edit.cshtml";
    private const string SeatsViewPath = "~/Views/Admin/Halls/Seats.cshtml";

    public HallsController(IHallService hallService, ICinemaService cinemaService)
    {
        _hallService = hallService;
        _cinemaService = cinemaService;
    }

    // GET: /Admin/Halls?cinemaId=1
    public async Task<IActionResult> Index(int? cinemaId)
    {
        var cinemaItems = await GetCinemaSelectListAsync(selectedId: cinemaId);

        List<HallListDto> halls;
        try
        {
            halls = cinemaId.HasValue
                ? await _hallService.GetByCinemaAsync(cinemaId.Value)
                : await _hallService.GetAllAsync();
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            halls = new List<HallListDto>();
        }

        var vm = new HallsIndexVm
        {
            CinemaId = cinemaId,
            Cinemas = cinemaItems,
            Halls = halls
        };

        return View(IndexViewPath, vm);
    }

    // GET: /Admin/Halls/Create
    [HttpGet]
    public async Task<IActionResult> Create(int? cinemaId)
    {
        var cinemas = await _cinemaService.GetAllAsync();

        if (cinemas.Count == 0)
        {
            TempData["Error"] = "Спочатку створіть кінотеатр.";
            return RedirectToAction("Create", "Cinemas", new { area = "Admin" });
        }

        var selected = cinemaId ?? cinemas[0].Id;

        var vm = new HallEditVm
        {
            CinemaId = selected,
            Cinemas = cinemas
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == selected))
                .ToList()
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
            await _hallService.CreateAsync(ToDto(vm));
            TempData["Success"] = "Зал успішно створено.";

            return RedirectToAction(nameof(Index), new { cinemaId = vm.CinemaId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
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

        var vm = ToVm(dto);
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
            await _hallService.UpdateAsync(ToDto(vm));
            TempData["Success"] = "Зал успішно оновлено.";

            return RedirectToAction(nameof(Index), new { cinemaId = vm.CinemaId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await FillCinemasAsync(vm);
            return View(EditViewPath, vm);
        }
    }

    // POST: /Admin/Halls/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? cinemaId)
    {
        try
        {
            await _hallService.DeleteAsync(id);
            TempData["Success"] = "Зал успішно видалено.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { cinemaId });
    }

    // GET: /Admin/Halls/Seats/5
    [HttpGet]
    public async Task<IActionResult> Seats(int id, int? rows, int? seatsPerRow)
    {
        var dto = await _hallService.GetSeatingAsync(id, rows, seatsPerRow);
        if (dto == null) return NotFound();

        ViewBag.SeatsError = TempData["SeatsError"] as string;
        ViewBag.Success = TempData["Success"] as string;
        ViewBag.Error = TempData["Error"] as string;

        return View(SeatsViewPath, dto);
    }

    // POST: /Admin/Halls/ConfigureSeats
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfigureSeats(int hallId, int rows, int seatsPerRow)
        => RedirectToAction(nameof(Seats), new { id = hallId, rows, seatsPerRow });

    // POST: /Admin/Halls/GenerateSeats
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateSeats(int hallId, bool allowRegenerate, List<RowSeatsDto> rowConfigs)
    {
        try
        {
            await _hallService.GenerateSeatsByConfigAsync(hallId, rowConfigs, allowRegenerate);
            TempData["Success"] = "Місця успішно згенеровано.";
        }
        catch (Exception ex)
        {
            TempData["SeatsError"] = ex.Message;
        }

        return RedirectToAction(nameof(Seats), new { id = hallId });
    }

    private static HallEditDto ToDto(HallEditVm vm) => new()
    {
        Id = vm.Id,
        CinemaId = vm.CinemaId,
        Name = vm.Name
    };

    private static HallEditVm ToVm(HallEditDto dto) => new()
    {
        Id = dto.Id,
        CinemaId = dto.CinemaId,
        Name = dto.Name
    };

    private async Task FillCinemasAsync(HallEditVm vm)
    {
        var cinemas = await _cinemaService.GetAllAsync();

        vm.Cinemas = cinemas
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == vm.CinemaId))
            .ToList();
    }

    private async Task<List<SelectListItem>> GetCinemaSelectListAsync(int? selectedId)
    {
        var cinemas = await _cinemaService.GetAllAsync();

        return cinemas
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = selectedId.HasValue && selectedId.Value == c.Id
            })
            .ToList();
    }
}
