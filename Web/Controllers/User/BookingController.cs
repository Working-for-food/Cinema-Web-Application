using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.ViewModels;

namespace Web.Controllers.User;

[Authorize]
public class BookingController : Controller
{
    private readonly SessionService _sessions;
    private readonly IBookingService _bookings;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CinemaDbContext _db;

    public BookingController(
        SessionService sessions,
        IBookingService bookings,
        UserManager<ApplicationUser> userManager,
        CinemaDbContext db)
    {
        _sessions = sessions;
        _bookings = bookings;
        _userManager = userManager;
        _db = db;
    }

    private string UserId => _userManager.GetUserId(User)!;

    private static string PresentationTypeUa(PresentationType t) => t switch
    {
        PresentationType.TwoD => "2D",
        PresentationType.ThreeD => "3D",
        PresentationType.Imax => "IMAX",
        _ => "2D"
    };

    /// <summary>
    /// Формує повний URL постера (TMDB або вже готовий URL)
    /// </summary>
    private static string? BuildPosterUrl(string? posterPath)
    {
        if (string.IsNullOrWhiteSpace(posterPath))
            return null;

        if (posterPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return posterPath;

        return "https://image.tmdb.org/t/p/w342" + posterPath;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int sessionId, CancellationToken ct)
    {
        await _sessions.EnsureSessionSeatsCreatedAsync(sessionId, ct);

        var seats = await _sessions.GetSeatsForBookingAsync(sessionId, ct);

        var session = await _db.Sessions
            .AsNoTracking()
            .Include(s => s.Movie)
            .Include(s => s.Hall)
                .ThenInclude(h => h.Cinema)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

        if (session is null)
            return NotFound();

        var vm = new BookingCreateVm
        {
            SessionId = sessionId,

            MovieTitle = session.Movie.Title,
            MoviePosterUrl = BuildPosterUrl(session.Movie.PosterPath),

            AgeLabel = session.Movie.AgeRating.HasValue
                ? $"{session.Movie.AgeRating.Value}+"
                : null,

            FormatLabel = PresentationTypeUa(session.PresentationType),

            LanguageLabel = string.IsNullOrWhiteSpace(session.Movie.Language)
                ? null
                : session.Movie.Language!.ToUpperInvariant(),

            HallName = session.Hall.Name,
            CinemaName = session.Hall.Cinema.Name,
            City = session.Hall.Cinema.City,

            StartTime = session.StartTime,
            EndTime = session.EndTime,

            Seats = seats.Select(s => new BookingCreateVm.SeatVm
            {
                SeatId = s.SeatId,
                RowNumber = s.RowNumber,
                SeatNumber = s.SeatNumber,
                Category = s.Category,
                Price = s.Price,
                IsBooked = s.BookingId != null
            }).ToList()
        };

        return View("~/Views/Bookings/Create.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateVm vm, CancellationToken ct)
    {
        var dto = new BookingCreateDto
        {
            SessionId = vm.SessionId,
            SeatIds = vm.SelectedSeatIds ?? new List<int>()
        };

        try
        {
            var result = await _bookings.CreateAsync(UserId, dto, ct);
            return RedirectToAction(nameof(Success), new { id = result.BookingId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);

            await _sessions.EnsureSessionSeatsCreatedAsync(vm.SessionId, ct);
            var seats = await _sessions.GetSeatsForBookingAsync(vm.SessionId, ct);

            var session = await _db.Sessions
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Cinema)
                .FirstOrDefaultAsync(s => s.Id == vm.SessionId, ct);

            if (session is not null)
            {
                vm.MovieTitle = session.Movie.Title;
                vm.MoviePosterUrl = BuildPosterUrl(session.Movie.PosterPath);
                vm.AgeLabel = session.Movie.AgeRating.HasValue
                    ? $"{session.Movie.AgeRating.Value}+"
                    : null;
                vm.FormatLabel = PresentationTypeUa(session.PresentationType);
                vm.LanguageLabel = string.IsNullOrWhiteSpace(session.Movie.Language)
                    ? null
                    : session.Movie.Language!.ToUpperInvariant();
                vm.HallName = session.Hall.Name;
                vm.CinemaName = session.Hall.Cinema.Name;
                vm.City = session.Hall.Cinema.City;
                vm.StartTime = session.StartTime;
                vm.EndTime = session.EndTime;
            }

            vm.Seats = seats.Select(s => new BookingCreateVm.SeatVm
            {
                SeatId = s.SeatId,
                RowNumber = s.RowNumber,
                SeatNumber = s.SeatNumber,
                Category = s.Category,
                Price = s.Price,
                IsBooked = s.BookingId != null
            }).ToList();

            return View("~/Views/Bookings/Create.cshtml", vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Success(int id, CancellationToken ct)
    {
        var dto = await _bookings.GetByIdAsync(id, ct);
        if (dto is null)
            return NotFound();

        var vm = new BookingSuccessVm
        {
            BookingId = dto.BookingId,
            TotalAmount = dto.TotalAmount,
            BookedAt = dto.BookedAt,
            Seats = dto.Seats.Select(s => $"{s.RowNumber}-{s.SeatNumber}").ToList()
        };

        return View("~/Views/Bookings/Success.cshtml", vm);
    }

    [HttpGet]
    public async Task<IActionResult> My(CancellationToken ct)
    {
        var items = await _bookings.GetMyAsync(UserId, ct);
        return View("~/Views/Bookings/My.cshtml", items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        try
        {
            await _bookings.CancelAsync(UserId, id, ct);
            TempData["Success"] = $"Бронювання №{id} скасовано.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(My));
    }
}
