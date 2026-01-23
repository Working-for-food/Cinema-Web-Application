using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.ViewModels.Admin.Cinemas;

namespace Web.Controllers.Admin;

[Area("Admin")]
public class CinemasController : Controller
{
    private readonly ICinemaService _cinemaService;

    private const string IndexViewPath = "~/Views/Admin/Cinemas/Index.cshtml";
    private const string CreateViewPath = "~/Views/Admin/Cinemas/Create.cshtml";
    private const string EditViewPath = "~/Views/Admin/Cinemas/Edit.cshtml";
    private const string DeleteViewPath = "~/Views/Admin/Cinemas/Delete.cshtml";
    private const string DetailsViewPath = "~/Views/Admin/Cinemas/Details.cshtml";

    public CinemasController(ICinemaService cinemaService)
    {
        _cinemaService = cinemaService;
    }

    // GET: /Admin/Cinemas?city=&search=&sort=
    public async Task<IActionResult> Index(string? city, string? search, string? sort)
    {
        city ??= "";
        search ??= "";
        sort ??= "";

        var cities = await _cinemaService.GetCitiesAsync();

        var vm = new CinemasIndexVm
        {
            City = city,
            Search = search,
            Sort = sort,

            Cities = cities.Select(c => new SelectListItem
            {
                Value = c,
                Text = c,
                Selected = c == city
            }).ToList(),

            SortOptions = new List<SelectListItem>
            {
                new("Name ↑", "") { Selected = string.IsNullOrEmpty(sort) },
                new("Name ↓", "name_desc") { Selected = sort == "name_desc" },
                new("City ↑", "city") { Selected = sort == "city" },
                new("City ↓", "city_desc") { Selected = sort == "city_desc" }
            },

            Cinemas = await _cinemaService.GetAllAsync(city, search, sort)
        };

        return View(IndexViewPath, vm);
    }

    // GET: /Admin/Cinemas/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var dto = await _cinemaService.GetDetailsAsync(id);
        if (dto == null) return NotFound();

        return View(DetailsViewPath, dto);
    }

    // GET: /Admin/Cinemas/Create
    [HttpGet]
    public IActionResult Create()
        => View(CreateViewPath, new CinemaEditVm());

    // POST: /Admin/Cinemas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CinemaEditVm vm)
    {
        if (!ModelState.IsValid)
            return View(CreateViewPath, vm);

        try
        {
            await _cinemaService.CreateAsync(new CinemaEditDto
            {
                Name = vm.Name,
                Address = vm.Address,
                City = vm.City
            });

            TempData["Success"] = "Cinema created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(CreateViewPath, vm);
        }
    }

    // GET: /Admin/Cinemas/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _cinemaService.GetForEditAsync(id);
        if (dto == null) return NotFound();

        return View(EditViewPath, new CinemaEditVm
        {
            Id = dto.Id,
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City
        });
    }

    // POST: /Admin/Cinemas/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CinemaEditVm vm)
    {
        if (!ModelState.IsValid)
            return View(EditViewPath, vm);

        try
        {
            await _cinemaService.UpdateAsync(new CinemaEditDto
            {
                Id = vm.Id,
                Name = vm.Name,
                Address = vm.Address,
                City = vm.City
            });

            TempData["Success"] = "Cinema updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(EditViewPath, vm);
        }
    }

    // GET: /Admin/Cinemas/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var dto = await _cinemaService.GetForEditAsync(id);
        if (dto == null) return NotFound();

        return View(DeleteViewPath, new CinemaEditVm
        {
            Id = dto.Id,
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City
        });
    }

    // POST: /Admin/Cinemas/DeleteConfirmed/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
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
