using Application.DTOs;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.ViewModels;

namespace Web.Controllers
{
    [Area("Admin")]
    public class SessionsController : Controller
    {
        private readonly SessionService _service;
        public SessionsController(SessionService service) => _service = service;

        public async Task<IActionResult> Index(DateTime? from, DateTime? to, CancellationToken ct)
        {
            var items = await _service.GetAllAsync(from, to, null, null, includeCancelled: true, ct);
            return View(items);
        }

        public IActionResult Create()
        {
            return View(new SessionEditVm { 
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2) 
            }); //Endtime - change
        }

        [HttpPost]
        public async Task<IActionResult> Create(SessionEditVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                await _service.CreateAsync(new SessionEditDto
                {
                    MovieId = vm.MovieId,
                    HallId = vm.HallId,
                    StartTime = vm.StartTime,
                    EndTime = vm.EndTime,
                    PresentationType = vm.PresentationType
                }, ct);

                TempData["success"] = "Session created";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            await _service.CancelAsync(id, ct);
            TempData["success"] = "Session cancelled";
            return RedirectToAction(nameof(Index));
        }
    }

}
