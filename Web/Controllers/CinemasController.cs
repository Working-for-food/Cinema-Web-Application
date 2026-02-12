using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.ViewModels.Cinemas;

namespace Web.Controllers;

public class CinemasController : Controller
{
    private readonly ICinemaService _cinemaService;

    public CinemasController(ICinemaService cinemaService)
    {
        _cinemaService = cinemaService;
    }

    // GET: /Cinemas
    public async Task<IActionResult> Index(string? city, string? search, CancellationToken ct)
    {
        var cities = await _cinemaService.GetCitiesAsync(ct);
        var cinemas = await _cinemaService.GetAllAsyncUser(city, search, null, ct);

        var vm = new UserCinemasIndexVm
        {
            SelectedCity = city,
            SearchQuery = search,
            Cities = cities.Select(c => new SelectListItem
            {
                Value = c,
                Text = c,
                Selected = string.Equals(c, city, StringComparison.OrdinalIgnoreCase)
            }).ToList(),
            Cinemas = (IEnumerable<Application.DTOs.CinemaDto>)cinemas
        };

        return View(vm);
    }

    // GET: /Cinemas/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var cinema = await _cinemaService.GetDetailsAsync(id, ct);
        if (cinema == null) return NotFound();

        return View(cinema);
    }
}