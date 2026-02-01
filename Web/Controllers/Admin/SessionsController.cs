using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.ViewModels.Admin.Sessions;

namespace Web.Controllers.Admin
{
    [Route("Admin/[controller]/[action]")]
    public class SessionsController : Controller
    {
        private readonly ISessionService _sessions;
        private readonly ISessionLookupService _lookups;

        private const string IndexViewPath = "~/Views/Admin/Sessions/Index.cshtml";
        private const string CreateViewPath = "~/Views/Admin/Sessions/Create.cshtml";
        private const string EditViewPath = "~/Views/Admin/Sessions/Edit.cshtml";
        private const string DetailsViewPath = "~/Views/Admin/Sessions/Details.cshtml";

        private static List<SelectListItem> BuildSortOptions(string? selected)
        {
            var items = new[]
            {
                ("start_asc",  "Початок ↑"),
                ("start_desc", "Початок ↓"),
                ("end_asc",    "Кінець ↑"),
                ("end_desc",   "Кінець ↓"),
            };

            return items.Select(x => new SelectListItem
            {
                Value = x.Item1,
                Text = x.Item2,
                Selected = string.Equals(selected, x.Item1, StringComparison.OrdinalIgnoreCase)
            }).ToList();
        }

        public SessionsController(ISessionService sessions, ISessionLookupService lookups)
        {
            _sessions = sessions;
            _lookups = lookups;
        }

        private static SessionEditDto ToDto(SessionEditVm vm)
        {
            if (vm.StartTime is null || vm.EndTime is null)
                throw new InvalidOperationException("Некоректно задані дата/час сеансу.");

            return new SessionEditDto
            {
                MovieId = vm.MovieId,
                HallId = vm.HallId,
                StartTime = vm.StartTime.Value,
                EndTime = vm.EndTime.Value,
                PresentationType = vm.PresentationType
            };
        }

        private static SessionEditVm ToEditVm(SessionDetailsDto s) => new()
        {
            Id = s.Id,

            CinemaId = s.CinemaId,
            HallId = s.HallId,
            MovieId = s.MovieId,

            StartTime = s.StartTime,
            EndTime = s.EndTime,

            PresentationType = s.PresentationType
        };

        private static List<SelectListItem> ToSelectList(IEnumerable<LookupItemDto> items, int? selectedId = null)
            => items.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Title,
                Selected = selectedId.HasValue && x.Id == selectedId.Value
            }).ToList();

        private static List<SelectListItem> BuildPresentationTypes(PresentationType selected)
            => Enum.GetValues<PresentationType>()
                .Select(p => new SelectListItem
                {
                    Value = p.ToString(),
                    Text = p switch
                    {
                        PresentationType.TwoD => "2D",
                        PresentationType.ThreeD => "3D",
                        _ => p.ToString()
                    },
                    Selected = p == selected
                }).ToList();

        private async Task FillEditLookupsAsync(SessionEditVm vm, CancellationToken ct)
        {
            var cinemas = await _lookups.GetCinemasAsync(ct);
            vm.Cinemas = ToSelectList(cinemas, vm.CinemaId == 0 ? null : vm.CinemaId);

            if (vm.CinemaId > 0)
            {
                var halls = await _lookups.GetHallsByCinemaAsync(vm.CinemaId, ct);
                vm.Halls = ToSelectList(halls, vm.HallId == 0 ? null : vm.HallId);

                if (vm.HallId > 0 && !vm.Halls.Any(h => h.Value == vm.HallId.ToString()))
                    vm.HallId = 0;
            }
            else
            {
                vm.HallId = 0;
                vm.Halls = new List<SelectListItem>();
            }

            var movies = await _lookups.GetMoviesAsync(query: null, ct);
            vm.Movies = ToSelectList(movies, vm.MovieId == 0 ? null : vm.MovieId);

            ViewBag.MovieDurations = movies
                .Where(x => x.DurationMinutes.HasValue && x.DurationMinutes.Value > 0)
                .ToDictionary(x => x.Id, x => x.DurationMinutes!.Value);

            ViewBag.MoviePosters = movies
                .Where(x => !string.IsNullOrWhiteSpace(x.PosterPath))
                .ToDictionary(x => x.Id, x => x.PosterPath!);

            vm.PresentationTypes = BuildPresentationTypes(vm.PresentationType);
        }

        private async Task FillIndexLookupsAsync(SessionsIndexVm vm, CancellationToken ct)
        {
            var cinemas = await _lookups.GetCinemasAsync(ct);
            vm.Cinemas = ToSelectList(cinemas, vm.CinemaId);

            if (!vm.CinemaId.HasValue)
            {
                vm.HallId = null;
                vm.Halls = new List<SelectListItem>();
            }
            else
            {
                var halls = await _lookups.GetHallsByCinemaAsync(vm.CinemaId.Value, ct);
                vm.Halls = ToSelectList(halls, vm.HallId);

                if (vm.HallId.HasValue && !vm.Halls.Any(h => h.Value == vm.HallId.Value.ToString()))
                    vm.HallId = null;
            }

            var movies = await _lookups.GetMoviesAsync(vm.MovieTitle, ct);
            vm.Movies = ToSelectList(movies, vm.MovieId);

            vm.SortOptions = BuildSortOptions(vm.Sort);
        }

        // GET: /Admin/Sessions/HallsByCinema?cinemaId=1
        [HttpGet]
        public async Task<IActionResult> HallsByCinema(int cinemaId, CancellationToken ct)
        {
            if (cinemaId < 1)
                return Ok(Array.Empty<object>());

            var halls = await _lookups.GetHallsByCinemaAsync(cinemaId, ct);

            return Ok(halls.Select(h => new { id = h.Id, title = h.Title }));
        }

        // GET: /Admin/Sessions/Index
        [HttpGet]
        public async Task<IActionResult> Index(SessionsIndexVm vm, CancellationToken ct = default)
        {
            if (vm.Page < 1) vm.Page = 1;

            await FillIndexLookupsAsync(vm, ct);

            var paged = await _sessions.GetAllPagedAsync(
                vm.From,
                vm.To,
                vm.CinemaId,
                vm.HallId,
                vm.MovieId,
                vm.IncludeCancelled,
                vm.IncludeFinished,
                vm.Sort,
                vm.Page,
                ct);

            vm.Sessions = paged.Items;
            vm.TotalCount = paged.TotalCount;
            vm.TotalPages = paged.TotalPages;

            return View(IndexViewPath, vm);
        }

        // GET: /Admin/Sessions/Details/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var dto = await _sessions.GetByIdAsync(id, ct);
            if (dto is null) return NotFound();

            var vm = new SessionDetailsVm
            {
                Id = dto.Id,
                MovieTitle = dto.MovieTitle,

                CinemaName = dto.CinemaName,
                HallName = dto.HallName,

                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                PresentationType = dto.PresentationType,
                IsCancelled = dto.IsCancelled,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };

            return View(DetailsViewPath, vm);
        }

        // GET: /Admin/Sessions/Create
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var now = DateTime.Now;
            now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute / 5 * 5, 0);

            var vm = new SessionEditVm
            {
                StartTime = now,
                EndTime = now.AddHours(2),
                PresentationType = PresentationType.TwoD
            };

            await FillEditLookupsAsync(vm, ct);
            return View(CreateViewPath, vm);
        }

        // POST: /Admin/Sessions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SessionEditVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await FillEditLookupsAsync(vm, ct);
                return View(CreateViewPath, vm);
            }

            try
            {
                await _sessions.CreateAsync(ToDto(vm), ct);
                TempData["Success"] = "Сеанс успішно створено.";

                vm.Id = null;

                await FillEditLookupsAsync(vm, ct);
                return View(CreateViewPath, vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await FillEditLookupsAsync(vm, ct);
                return View(CreateViewPath, vm);
            }
        }

        // GET: /Admin/Sessions/Edit/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var s = await _sessions.GetByIdAsync(id, ct);
            if (s is null) return NotFound();

            var vm = ToEditVm(s);

            await FillEditLookupsAsync(vm, ct);
            ViewBag.MovieTitle = s.MovieTitle;

            return View(EditViewPath, vm);
        }

        // POST: /Admin/Sessions/Edit/5
        [HttpPost("{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SessionEditVm vm, CancellationToken ct)
        {
            vm.Id = id;

            if (!ModelState.IsValid)
            {
                await FillEditLookupsAsync(vm, ct);
                return View(EditViewPath, vm);
            }

            try
            {
                var ok = await _sessions.UpdateAsync(id, ToDto(vm), ct);
                if (!ok) return NotFound();

                TempData["Success"] = "Сеанс успішно оновлено.";

                await FillEditLookupsAsync(vm, ct);
                ViewBag.MovieTitle = vm.MovieId > 0
                            ? await _lookups.GetMovieTitleByIdAsync(vm.MovieId, ct) ?? ""
                            : "";

                return View(EditViewPath, vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await FillEditLookupsAsync(vm, ct);
                return View(EditViewPath, vm);
            }
        }

        // POST: /Admin/Sessions/Cancel/5
        [HttpPost("{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? returnUrl, CancellationToken ct)
        {
            var ok = await _sessions.CancelAsync(id, ct);
            if (!ok) return NotFound();

            TempData["Success"] = "Сеанс скасовано.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Sessions/Restore/5
        [HttpPost("{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id, string? returnUrl, CancellationToken ct)
        {
            try
            {
                var ok = await _sessions.RestoreAsync(id, ct);
                if (!ok) return NotFound();

                TempData["Success"] = "Сеанс відновлено.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }
    }
}
