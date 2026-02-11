using Application.DTOs;
using Application.DTOs.Pricing;
using Application.Interfaces;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
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
        private readonly IPricingTemplateService _templateService;

        private const string IndexViewPath = "~/Views/Admin/Sessions/Index.cshtml";
        private const string CreateViewPath = "~/Views/Admin/Sessions/Create.cshtml";
        private const string EditViewPath = "~/Views/Admin/Sessions/Edit.cshtml";
        private const string DetailsViewPath = "~/Views/Admin/Sessions/Details.cshtml";
        private const string PricingViewPath = "~/Views/Admin/Sessions/Pricing.cshtml";

        private static string CategoryTitle(SeatCategory c) => c switch
        {
            SeatCategory.Standard => "Звичайне",
            SeatCategory.Vip => "VIP",
            SeatCategory.Accessible => "Інклюзивне",
            _ => c.ToString()
        };

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

        public SessionsController(ISessionService sessions, ISessionLookupService lookups, IPricingTemplateService templateService)
        {
            _sessions = sessions;
            _lookups = lookups;
            _templateService = templateService;
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

        private static SessionPricingDto ToPricingDto(SessionEditVm vm)
        {
            var rowPrices = (vm.RowPrices ?? new())
                .Select(x => new RowPriceDto
                {
                    Row = x.RowNumber,
                    BasePrice = x.BasePrice
                })
                .ToList();

            var multipliers = (vm.CategoryMultipliers ?? new())
                .Select(x => new CategoryMultiplierDto
                {
                    Category = x.Category,
                    Multiplier = x.Multiplier
                })
                .ToList();

            return new SessionPricingDto
            {
                SessionId = vm.Id ?? 0,
                HallId = vm.HallId,
                RowPrices = rowPrices,
                CategoryMultipliers = multipliers
            };
        }

        // GET: /Admin/Sessions/PricingMeta?hallId=1
        [HttpGet]
        public async Task<IActionResult> PricingMeta(int hallId, CancellationToken ct)
        {
            if (hallId < 1)
                return Ok(new
                {
                    rows = Array.Empty<int>(),
                    categories = Array.Empty<object>(),
                    seats = Array.Empty<object>(),
                    maxSeats = 0
                });

            var seats = await _lookups.GetHallSeatsAsync(hallId, ct);

            if (seats.Count == 0)
                return Ok(new
                {
                    rows = Array.Empty<int>(),
                    categories = Array.Empty<object>(),
                    seats = Array.Empty<object>(),
                    maxSeats = 0
                });

            var rows = seats.Select(s => s.RowNumber)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            var categories = seats.Select(s => s.Category)
                .Distinct()
                .OrderBy(x => x)
                .Select(c => new { id = (int)c, title = CategoryTitle(c) })
                .ToArray();

            var maxSeats = seats.GroupBy(s => s.RowNumber)
                .Select(g => g.Max(x => x.SeatNumber))
                .DefaultIfEmpty(0)
                .Max();

            return Ok(new
            {
                rows,
                categories,
                maxSeats,
                seats = seats.Select(s => new
                {
                    id = s.Id,
                    row = s.RowNumber,
                    number = s.SeatNumber,
                    category = (int)s.Category
                })
            });
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
            await _sessions.EnsureSessionSeatsCreatedAsync(id, ct);

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
                var id = await _sessions.CreateAsync(ToDto(vm), ct);
                await _sessions.ApplyPricingAsync(id, ToPricingDto(vm), ct);

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
            await _sessions.EnsureSessionSeatsCreatedAsync(id, ct);

            var s = await _sessions.GetByIdAsync(id, ct);
            if (s is null) return NotFound();

            var vm = ToEditVm(s);
            var pricing = await _sessions.GetPricingAsync(id, ct);

            vm.RowPrices = pricing.RowPrices
                .Select(x => new SessionEditVm.RowPriceVm
                {
                    RowNumber = x.Row,
                    BasePrice = x.BasePrice
                })
                .ToList();

            vm.CategoryMultipliers = pricing.CategoryMultipliers
                .Select(x => new SessionEditVm.CategoryMultiplierVm
                {
                    Category = x.Category,
                    Multiplier = x.Multiplier
                })
                .ToList();

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

                await _sessions.ApplyPricingAsync(id, ToPricingDto(vm), ct);

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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Pricing(int id, CancellationToken ct)
        {
            await _sessions.EnsureSessionSeatsCreatedAsync(id, ct);

            var s = await _sessions.GetByIdAsync(id, ct);
            if (s is null) return NotFound();

            var pricing = await _sessions.GetPricingAsync(id, ct);
            var seats = await _sessions.GetSeatPricesAsync(id, ct);

            var vm = new SessionPricingPageVm
            {
                SessionId = id,
                MovieTitle = s.MovieTitle,
                CinemaName = s.CinemaName,
                HallName = s.HallName,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                RowPrices = pricing.RowPrices
                    .Select(x => new SessionPricingPageVm.RowPriceVm { RowNumber = x.Row, BasePrice = x.BasePrice })
                    .ToList(),
                CategoryMultipliers = pricing.CategoryMultipliers
                .Select(x => new SessionPricingPageVm.CategoryMultiplierVm
                {
                    Category = x.Category,
                    Multiplier = x.Multiplier,
                    Title = CategoryTitle((SeatCategory)x.Category)
                })
                .ToList(),
                Seats = seats.Select(x => new SessionPricingPageVm.SeatPriceVm
                {
                    SeatId = x.SeatId,
                    Row = x.Row,
                    Number = x.Number,
                    Category = x.Category,
                    Price = x.Price
                }).ToList()
            };

            ViewBag.HallId = s.HallId;
            ViewBag.HasBookings = await _sessions.HasBookingsAsync(id, ct);

            return View(PricingViewPath, vm);
        }

        [HttpPost("{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pricing(int id, SessionPricingPageVm vm, CancellationToken ct)
        {
            vm.SessionId = id;

            var s = await _sessions.GetByIdAsync(id, ct);
            if (s is null) return NotFound();

            async Task<IActionResult> ReturnWithViewModelAsync()
            {
                vm.MovieTitle = s.MovieTitle;
                vm.CinemaName = s.CinemaName;
                vm.HallName = s.HallName;
                vm.StartTime = s.StartTime;
                vm.EndTime = s.EndTime;

                var seats = await _sessions.GetSeatPricesAsync(id, ct);
                vm.Seats = seats.Select(x => new SessionPricingPageVm.SeatPriceVm
                {
                    SeatId = x.SeatId,
                    Row = x.Row,
                    Number = x.Number,
                    Category = x.Category,
                    Price = x.Price
                }).ToList();

                if (vm.CategoryMultipliers != null)
                {
                    foreach (var cm in vm.CategoryMultipliers)
                        cm.Title = CategoryTitle((SeatCategory)cm.Category);
                }

                ViewBag.HallId = s.HallId;
                ViewBag.HasBookings = await _sessions.HasBookingsAsync(id, ct);

                return View(PricingViewPath, vm);
            }

            if (!ModelState.IsValid)
                return await ReturnWithViewModelAsync();

            if (await _sessions.HasBookingsAsync(id, ct))
            {
                ModelState.AddModelError(string.Empty, "Неможливо змінити ціни, оскільки на цей сеанс вже є продані квитки.");
                return await ReturnWithViewModelAsync();
            }

            try
            {
                var dto = new SessionPricingDto
                {
                    SessionId = id,
                    HallId = s.HallId,
                    RowPrices = (vm.RowPrices ?? new())
                        .Select(x => new RowPriceDto
                        {
                            Row = x.RowNumber,
                            BasePrice = x.BasePrice
                        })
                        .ToList(),
                    CategoryMultipliers = (vm.CategoryMultipliers ?? new())
                        .Select(x => new CategoryMultiplierDto
                        {
                            Category = x.Category,
                            Multiplier = x.Multiplier
                        })
                        .ToList()
                };

                await _sessions.ApplyPricingAsync(id, dto, ct);

                TempData["Success"] = "Ціни збережено.";
                return RedirectToAction(nameof(Pricing), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return await ReturnWithViewModelAsync();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTemplatesByHall(int hallId, CancellationToken ct)
        {
            if (hallId < 1) return Ok(Array.Empty<object>());

            var templates = await _templateService.GetActiveListForHallAsync(hallId, ct);
            return Ok(templates.Select(t => new { id = t.Id, title = t.Name }));
        }

        // GET: /Admin/Sessions/LoadTemplateData?templateId=5
        [HttpGet]
        public async Task<IActionResult> LoadTemplateData(int templateId, CancellationToken ct)
        {
            var data = await _templateService.GetTemplateDataAsync(templateId, ct);
            if (data is null) return NotFound();

            return Ok(data);
        }
    }
}
