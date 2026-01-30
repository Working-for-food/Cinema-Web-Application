using Application.Interfaces;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.ViewModels.Admin.People;

namespace Web.Controllers.Admin;

[Route("admin/people")]
public class PeopleController : Controller
{
    private const string ViewsRoot = "~/Views/Admin/People";
    private readonly IPersonService _people;
    private readonly ICountryLookupService _countries;

    public PeopleController(IPersonService people, ICountryLookupService countries)
    {
        _people = people;
        _countries = countries;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, int page = 1, CancellationToken ct = default)
    {
        var (items, total) = await _people.GetAllAsync(search, page < 1 ? 1 : page, 10, ct);
        ViewBag.Search = search;
        ViewBag.Page = page < 1 ? 1 : page;
        ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(total / 10.0));

        return View($"{ViewsRoot}/Index.cshtml", items);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var vm = new PersonEditVm();
        await FillCountriesAsync(vm, ct);
        return View($"{ViewsRoot}/Create.cshtml", vm);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PersonEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await FillCountriesAsync(vm, ct);
            return View($"{ViewsRoot}/Create.cshtml", vm);
        }

        var entity = Map(vm);
        var (ok, error) = await _people.CreateAsync(entity, ct);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            await FillCountriesAsync(vm, ct);
            return View($"{ViewsRoot}/Create.cshtml", vm);
        }

        TempData["Success"] = "Person created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var p = await _people.GetByIdAsync(id, ct);
        if (p == null) return NotFound();

        var vm = new PersonEditVm
        {
            Id = p.Id,
            FirstName = p.FirstName,
            MiddleName = p.MiddleName,
            LastName = p.LastName,
            BirthDate = p.BirthDate,
            CountryCode = p.CountryCode,
            PhotoUrl = p.PhotoUrl
        };

        await FillCountriesAsync(vm, ct);
        return View($"{ViewsRoot}/Edit.cshtml", vm);
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PersonEditVm vm, CancellationToken ct)
    {
        vm.Id = id;

        if (!ModelState.IsValid)
        {
            await FillCountriesAsync(vm, ct);
            return View($"{ViewsRoot}/Edit.cshtml", vm);
        }

        var entity = Map(vm);
        var (ok, error) = await _people.UpdateAsync(entity, ct);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            await FillCountriesAsync(vm, ct);
            return View($"{ViewsRoot}/Edit.cshtml", vm);
        }

        TempData["Success"] = "Person updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var (ok, error) = await _people.DeleteAsync(id, ct);
        TempData[ok ? "Success" : "Error"] = ok ? "Person deleted." : error;
        return RedirectToAction(nameof(Index));
    }

    private async Task FillCountriesAsync(PersonEditVm vm, CancellationToken ct)
    {
        var countries = await _countries.GetAllAsync(ct);
        vm.CountryList = new SelectList(countries, "Code", "Name", vm.CountryCode);
    }

    private static Person Map(PersonEditVm vm) => new()
    {
        Id = vm.Id,
        FirstName = vm.FirstName,
        MiddleName = vm.MiddleName,
        LastName = vm.LastName,
        BirthDate = vm.BirthDate,
        CountryCode = vm.CountryCode,
        PhotoUrl = vm.PhotoUrl
    };
}
