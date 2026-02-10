using Application.Interfaces;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.ViewModels.Schedule;

namespace Web.Controllers;

[Route("schedule")]
public sealed class ScheduleController : Controller
{
    private readonly IScheduleService _schedule;
    private readonly ICinemaService _cinemas;

    public ScheduleController(IScheduleService schedule, ICinemaService cinemas)
    {
        _schedule = schedule;
        _cinemas = cinemas;
    }

    // GET /schedule/choose?city=Kyiv&cinemaId=1
    [HttpGet("choose")]
    public async Task<IActionResult> Choose(string? city, int? cinemaId, CancellationToken ct)
    {
        city = string.IsNullOrWhiteSpace(city) ? null : city.Trim();

        var cities = await _cinemas.GetCitiesAsync(ct);

        var vm = new ScheduleChooseVm
        {
            City = city,
            CinemaId = cinemaId,
            Cities = cities.Select(c => new SelectListItem
            {
                Value = c,
                Text = c,
                Selected = city != null && string.Equals(city, c, StringComparison.OrdinalIgnoreCase)
            }).ToList()
        };

        if (city != null)
        {
            var cinemas = await _cinemas.GetAllAsync(city: city, ct: ct);

            vm.Cinemas = cinemas.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(c.Address) ? c.Name : $"{c.Name} — {c.Address}",
                Selected = cinemaId.HasValue && cinemaId.Value == c.Id
            }).ToList();
        }

        return View("~/Views/Schedule/Choose.cshtml", vm);
    }


    // GET /schedule?cinemaId=1&date=2026-02-10
    [HttpGet("")]
    public async Task<IActionResult> Index(int cinemaId, DateOnly? date, CancellationToken ct)
    {
        if (cinemaId < 1)
            return RedirectToAction(nameof(Choose));

        var dto = await _schedule.GetCinemaScheduleAsync(cinemaId, date, ct);

        var vm = new CinemaScheduleVm
        {
            CinemaId = dto.CinemaId,
            CinemaName = dto.CinemaName,
            SelectedDate = dto.SelectedDate,
            Days = dto.Days.Select(d => new CinemaScheduleVm.ScheduleDayVm
            {
                Date = d.Date,
                LabelTop = d.LabelTop,
                LabelBottom = d.LabelBottom,
                IsActive = d.IsActive
            }).ToList(),
            Movies = dto.Movies.Select(m => new CinemaScheduleVm.MovieScheduleCardVm
            {
                MovieId = m.MovieId,
                Title = m.Title,
                PosterUrl = m.PosterUrl,
                AgeLabel = m.AgeLabel,
                Times = m.Times.Select(t => new CinemaScheduleVm.SessionTimeVm
                {
                    SessionId = t.SessionId,
                    Time = t.Time,
                    PresentationType = t.PresentationType
                }).ToList()
            }).ToList()
        };

        return View("~/Views/Schedule/Index.cshtml", vm);
    }

    // GET /schedule/locations (поки заглушка)
    [HttpGet("locations")]
    public IActionResult Locations()
        => View("~/Views/Schedule/Locations.cshtml");
}
