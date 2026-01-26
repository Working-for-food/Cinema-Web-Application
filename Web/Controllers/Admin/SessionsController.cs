using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")]
    public class SessionsController : Controller
    {
        private readonly ISessionService _sessions;
        private readonly ISessionLookupService _lookups;

        private async Task FillLookupsAsync(int? selectedHallId, CancellationToken ct)
        {
            var halls = await _lookups.GetHallsAsync(ct);

            ViewBag.Halls = halls.Select(h => new SelectListItem
            {
                Value = h.Id.ToString(),
                Text = h.Title,
                Selected = selectedHallId.HasValue && h.Id == selectedHallId.Value
            }).ToList();

            ViewBag.Movies = await _lookups.GetMoviesAsync(null, ct);
        }

        public SessionsController(ISessionService sessions, ISessionLookupService lookups)
        {
            _sessions = sessions;
            _lookups = lookups;
        }

        // GET: /Admin/Sessions
        public async Task<IActionResult> Index(
            DateTime? from,
            DateTime? to,
            int? hallId,
            int? movieId,
            bool includeCancelled = true,
            CancellationToken ct = default)
        {
            var list = await _sessions.GetAllAsync(from, to, hallId, movieId, includeCancelled, ct);

            await FillLookupsAsync(hallId, ct);

            return View(list);
}


        // GET: /Admin/Sessions/Details/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var dto = await _sessions.GetByIdAsync(id, ct);
            if (dto is null) return NotFound();
            return View(dto);
        }

        // GET: /Admin/Sessions/Create
        [HttpGet]
        public IActionResult Create()
        {
            var now = DateTime.Now;

            now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute / 5 * 5, 0);

            var dto = new SessionEditDto
            {
                StartTime = now,
                EndTime = now.AddHours(2)
            };

            return View(dto);
        }

        // POST: /Admin/Sessions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SessionEditDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                var id = await _sessions.CreateAsync(dto, ct);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET: /Admin/Sessions/Edit/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var s = await _sessions.GetByIdAsync(id, ct);
            if (s is null) return NotFound();

            var dto = new SessionEditDto
            {
                MovieId = s.MovieId,
                HallId = s.HallId,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                PresentationType = s.PresentationType
            };

            return View(dto);
        }

        // POST: /Admin/Sessions/Edit/5
        [HttpPost("{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SessionEditDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                var ok = await _sessions.UpdateAsync(id, dto, ct);
                if (!ok) return NotFound();

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // POST: /Admin/Sessions/Cancel/5
        [HttpPost("{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            var ok = await _sessions.CancelAsync(id, ct);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Sessions/Restore/5
        [HttpPost("{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id, CancellationToken ct)
        {
            try
            {
                var ok = await _sessions.RestoreAsync(id, ct);
                if (!ok) return NotFound();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
