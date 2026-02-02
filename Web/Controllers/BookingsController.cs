using Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Bookings;

namespace Web.Controllers;

public class BookingsController : Controller
{
    [HttpGet]
    public IActionResult Create(int sessionId = 1)
    {
        var vm = new BookingCreateVm
        {
            SessionId = sessionId,
            MovieTitle = "Demo Movie",
            CinemaName = "Demo Cinema",
            HallName = "Hall 1",
            StartTime = DateTime.Now.AddHours(2),
            PresentationType = PresentationType.TwoD,
            Seats = BuildDemoSeats()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(BookingCreateVm vm)
    {
        if (vm.SeatIds == null || vm.SeatIds.Count == 0)
        {
            TempData["Error"] = "Оберіть хоча б одне місце.";
            return RedirectToAction(nameof(Create), new { sessionId = vm.SessionId });
        }

        return RedirectToAction(nameof(Success), new { id = 123 });
    }

    public IActionResult Success(int id)
    {
        var vm = new BookingSuccessVm
        {
            BookingId = id,
            SessionId = 1,
            TotalAmount = 540m,
            BookedAt = DateTime.Now,
            MovieTitle = "Demo Movie",
            CinemaName = "Demo Cinema",
            HallName = "Hall 1",
            SeatsText = new() { "Ряд 3, Місце 7", "Ряд 3, Місце 8" }
        };

        return View(vm);
    }

    public IActionResult My()
    {
        return View(new MyBookingsVm());
    }

    private static List<BookingCreateVm.SeatVm> BuildDemoSeats()
    {
        var list = new List<BookingCreateVm.SeatVm>();
        var id = 1;

        for (int r = 1; r <= 6; r++)
        {
            for (int s = 1; s <= 8; s++)
            {
                list.Add(new BookingCreateVm.SeatVm
                {
                    SeatId = id++,
                    RowNumber = r,
                    SeatNumber = s,
                    Category = r <= 2 ? SeatCategory.Vip : SeatCategory.Standard,
                    Price = r <= 2 ? 250m : 180m,
                    IsBooked = (r == 2 && s is 3 or 4) || (r == 5 && s == 7)
                });
            }
        }
        return list;
    }
}
