using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.ViewModels.Admin.Cinemas;

namespace Web.Controllers.Admin;

[Area("Admin")]
public class CinemasController : Controller
{
    private readonly ICinemaService _cinemaService;
    private readonly Cloudinary _cloudinary;

    private const string IndexViewPath = "~/Views/Admin/Cinemas/Index.cshtml";
    private const string CreateViewPath = "~/Views/Admin/Cinemas/Create.cshtml";
    private const string EditViewPath = "~/Views/Admin/Cinemas/Edit.cshtml";
    private const string DeleteViewPath = "~/Views/Admin/Cinemas/Delete.cshtml";
    private const string DetailsViewPath = "~/Views/Admin/Cinemas/Details.cshtml";

    public CinemasController(ICinemaService cinemaService, Cloudinary cloudinary)
    {
        _cinemaService = cinemaService;
        _cloudinary = cloudinary;
    }

    // GET: /Admin/Cinemas?city=&search=&sort=
    public async Task<IActionResult> Index(string? city, string? search, string? sort, CancellationToken ct)
    {
        city = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        sort = string.IsNullOrWhiteSpace(sort) ? null : sort.Trim();

        var cities = await _cinemaService.GetCitiesAsync(ct);
        var cinemas = await _cinemaService.GetAllAsync(city, search, sort, ct);

        var vm = new CinemasIndexVm
        {
            City = city ?? "",
            Search = search ?? "",
            Sort = sort ?? "",

            Cities = BuildCitiesSelectList(cities, city),

            SortOptions = new List<SelectListItem>
            {
                new("За назвою (А→Я)", "") { Selected = string.IsNullOrEmpty(sort) },
                new("За назвою (Я→А)", "name_desc") { Selected = sort == "name_desc" },
                new("За містом (А→Я)", "city") { Selected = sort == "city" },
                new("За містом (Я→А)", "city_desc") { Selected = sort == "city_desc" }
            },

            Cinemas = cinemas
        };

        return View(IndexViewPath, vm);
    }

    // GET: /Admin/Cinemas/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var dto = await _cinemaService.GetDetailsAsync(id, ct);
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
    public async Task<IActionResult> Create(CinemaEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(CreateViewPath, vm);

        try
        {
            // ✅ Upload в Cloudinary (якщо файл вибраний)
            if (vm.Image != null && vm.Image.Length > 0)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(vm.Image.FileName, vm.Image.OpenReadStream()),
                    Folder = "cinemas",
                    Transformation = new Transformation().Height(500).Width(500).Crop("fill")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams, ct);

                if (uploadResult.Error != null)
                    throw new DomainException(uploadResult.Error.Message);

                vm.ImageUrl = uploadResult.SecureUrl?.ToString();
            }

            await _cinemaService.CreateAsync(ToDto(vm), ct);

            TempData["Success"] = "Кінотеатр успішно створено.";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(CreateViewPath, vm);
        }
    }

    // GET: /Admin/Cinemas/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var dto = await _cinemaService.GetForEditAsync(id, ct);
        if (dto == null) return NotFound();

        return View(EditViewPath, ToVm(dto));
    }

    // POST: /Admin/Cinemas/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CinemaEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(EditViewPath, vm);

        try
        {
            // ✅ Upload тільки якщо вибрали новий файл
            if (vm.Image != null && vm.Image.Length > 0)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(vm.Image.FileName, vm.Image.OpenReadStream()),
                    Folder = "cinemas",
                    Transformation = new Transformation().Height(500).Width(500).Crop("fill")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams, ct);

                if (uploadResult.Error != null)
                    throw new DomainException(uploadResult.Error.Message);

                vm.ImageUrl = uploadResult.SecureUrl?.ToString();
            }
            // якщо файл не вибрали — vm.ImageUrl лишається тим, що прийшло з форми (hidden/або прив’язка)

            await _cinemaService.UpdateAsync(ToDto(vm), ct);

            TempData["Success"] = "Кінотеатр успішно оновлено.";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(EditViewPath, vm);
        }
    }

    // GET: /Admin/Cinemas/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var dto = await _cinemaService.GetForEditAsync(id, ct);
        if (dto == null) return NotFound();

        return View(DeleteViewPath, ToVm(dto));
    }

    // POST: /Admin/Cinemas/DeleteConfirmed/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        try
        {
            await _cinemaService.DeleteAsync(id, ct);
            TempData["Success"] = "Кінотеатр успішно видалено.";
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private static CinemaEditDto ToDto(CinemaEditVm vm) => new()
    {
        Id = vm.Id,
        Name = vm.Name,
        Address = vm.Address,
        City = vm.City,
        ImageUrl = vm.ImageUrl // ✅ додано
    };

    private static CinemaEditVm ToVm(CinemaEditDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Address = dto.Address,
        City = dto.City,
        ImageUrl = dto.ImageUrl // ✅ додано
    };

    private static List<SelectListItem> BuildCitiesSelectList(List<string> cities, string? selectedCity)
    {
        var list = new List<SelectListItem>
        {
            new()
            {
                Value = "",
                Text = "Усі міста",
                Selected = string.IsNullOrEmpty(selectedCity)
            }
        };

        list.AddRange(cities.Select(c => new SelectListItem
        {
            Value = c,
            Text = c,
            Selected = string.Equals(c, selectedCity, StringComparison.OrdinalIgnoreCase)
        }));

        return list;
    }
}
