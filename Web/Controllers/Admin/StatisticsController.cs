using Application.Interfaces;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.ViewModels.Admin.Statistics;

namespace Web.Controllers.Admin;

[Route("Admin/[controller]/[action]")]
public class StatisticsController : Controller
{
    private readonly IBookingStatisticsService _stats;
    private readonly ISessionLookupService _lookups;

    private const string IndexViewPath = "~/Views/Admin/Statistics/Index.cshtml";

    public StatisticsController(IBookingStatisticsService stats, ISessionLookupService lookups)
    {
        _stats = stats;
        _lookups = lookups;
    }

    private static List<SelectListItem> ToSelectList(IEnumerable<Application.DTOs.LookupItemDto> items, int selectedId = 0)
        => items.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Title,
            Selected = selectedId > 0 && x.Id == selectedId
        }).ToList();

    private async Task FillLookupsAsync(BookingStatisticsIndexVm vm, CancellationToken ct)
    {
        var cinemas = await _lookups.GetCinemasAsync(ct);
        vm.Cinemas = ToSelectList(cinemas, vm.CinemaId);

        if (vm.CinemaId > 0)
        {
            var halls = await _lookups.GetHallsByCinemaAsync(vm.CinemaId, ct);
            vm.Halls = ToSelectList(halls, vm.HallId);
        }
        else
        {
            vm.Halls = new List<SelectListItem>();
            vm.HallId = 0;
        }

        var movies = await _lookups.GetMoviesAsync(query: null, ct);
        vm.Movies = ToSelectList(movies, vm.MovieId);

        if (vm.MovieId > 0 && string.IsNullOrWhiteSpace(vm.MovieTitle))
            vm.MovieTitle = await _lookups.GetMovieTitleByIdAsync(vm.MovieId, ct) ?? "";
    }

    [HttpGet]
    public async Task<IActionResult> Index(BookingStatisticsIndexVm vm, CancellationToken ct)
    {
        
        if (!vm.SessionFrom.HasValue && !vm.SessionTo.HasValue && !vm.BookingFrom.HasValue && !vm.BookingTo.HasValue)
        {
            vm.SessionFrom = DateTime.Today.AddDays(-30);
            vm.SessionTo = DateTime.Today.AddDays(30);

            
            vm.BookingFrom = DateTime.UtcNow.Date.AddDays(-30);
            vm.BookingTo = DateTime.UtcNow;
        }

        await FillLookupsAsync(vm, ct);

        
        if (vm.SessionFrom.HasValue && vm.SessionTo.HasValue && vm.SessionFrom > vm.SessionTo)
            (vm.SessionFrom, vm.SessionTo) = (vm.SessionTo, vm.SessionFrom);

        if (vm.BookingFrom.HasValue && vm.BookingTo.HasValue && vm.BookingFrom > vm.BookingTo)
            (vm.BookingFrom, vm.BookingTo) = (vm.BookingTo, vm.BookingFrom);

        var filter = new BookingStatisticsFilter
        {
            SessionFrom = vm.SessionFrom,
            SessionTo = vm.SessionTo,

            CinemaId = vm.CinemaId > 0 ? vm.CinemaId : null,
            HallId = vm.HallId > 0 ? vm.HallId : null,
            MovieId = vm.MovieId > 0 ? vm.MovieId : null,
            SessionId = vm.SessionId is > 0 ? vm.SessionId : null,

            PresentationTypes = vm.PresentationTypes,
            SeatCategories = vm.SeatCategories,

            IncludeCancelledSessions = vm.IncludeCancelledSessions,
            IncludeFinishedSessions = vm.IncludeFinishedSessions,
            BookingFrom = vm.BookingFrom,
            BookingTo = vm.BookingTo,
            IncludeDeletedBookingsInPeriod = vm.IncludeDeletedBookingsInPeriod,

            
            DayGroupingMode = StatisticsDateMode.SessionStart
        };

        vm.Result = await _stats.GetBookingStatisticsAsync(filter, ct);
        return View(IndexViewPath, vm);
    }
}
