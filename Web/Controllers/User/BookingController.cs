using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels;

namespace Web.Controllers.User;

[Authorize]
public class BookingController : Controller
{
    private readonly ISessionService _sessions;
    private readonly IBookingService _bookings;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingController(
        ISessionService sessions,
        IBookingService bookings,
        UserManager<ApplicationUser> userManager)
    {
        _sessions = sessions;
        _bookings = bookings;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int sessionId, CancellationToken ct)
    {
        var seats = await _sessions.GetSeatsForBookingAsync(sessionId, ct);

        var vm = new BookingCreateVm
        {
            SessionId = sessionId,
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
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Unauthorized();

        var dto = new BookingCreateDto
        {
            SessionId = vm.SessionId,
            SeatIds = vm.SelectedSeatIds ?? new List<int>()
        };

        try
        {
            var result = await _bookings.CreateAsync(userId, dto, ct);
            return RedirectToAction(nameof(Success), new { id = result.BookingId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);

            // перезагрузим места (чтобы актуально показать занято)
            var seats = await _sessions.GetSeatsForBookingAsync(vm.SessionId, ct);
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
        if (dto is null) return NotFound();

        var vm = new BookingSuccessVm
        {
            BookingId = dto.BookingId,
            TotalAmount = dto.TotalAmount,
            BookedAt = dto.BookedAt,
            Seats = dto.Seats
                .Select(s => $"{s.RowNumber}-{s.SeatNumber}")
                .ToList()
        };

        return View("~/Views/Bookings/Success.cshtml", vm);
    }

    [HttpGet]
    public async Task<IActionResult> My(CancellationToken ct)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Unauthorized();

        var items = await _bookings.GetMyAsync(userId, ct);
        return View("~/Views/Bookings/My.cshtml", items);
    }
}
