using Application.DTOs;
using Application.DTOs.Pricing;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.ViewModels.Admin.PricingTemplates;

namespace Web.Controllers.Admin
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    public class PricingTemplatesController : Controller
    {
        private readonly IPricingTemplateService _service;
        private readonly ISessionLookupService _lookups;

        private const string IndexViewPath = "~/Views/Admin/PricingTemplates/Index.cshtml";
        private const string CreateViewPath = "~/Views/Admin/PricingTemplates/Create.cshtml";
        private const string EditViewPath = "~/Views/Admin/PricingTemplates/Edit.cshtml";

        public PricingTemplatesController(IPricingTemplateService service, ISessionLookupService lookups)
        {
            _service = service;
            _lookups = lookups;
        }

        private static List<SelectListItem> ToSelectList(IEnumerable<LookupItemDto> items, int? selectedId)
             => items.Select(x => new SelectListItem
             {
                 Value = x.Id.ToString(),
                 Text = x.Title,
                 Selected = selectedId.HasValue && x.Id == selectedId.Value
             }).ToList();

        private async Task FillIndexLookupsAsync(PricingTemplatesIndexVm vm, CancellationToken ct)
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
        }

        private async Task FillEditLookupsAsync(PricingTemplateEditVm vm, CancellationToken ct)
        {
            var cinemas = await _lookups.GetCinemasAsync(ct);
            vm.Cinemas = ToSelectList(cinemas, vm.CinemaId);

            if (vm.CinemaId.HasValue)
            {
                var halls = await _lookups.GetHallsByCinemaAsync(vm.CinemaId.Value, ct);
                vm.Halls = ToSelectList(halls, vm.HallId);
            }
            else
            {
                vm.Halls = new List<SelectListItem>();
            }
        }

        // GET: /Admin/PricingTemplates/Index
        [HttpGet]
        public async Task<IActionResult> Index(PricingTemplatesIndexVm vm, CancellationToken ct)
        {
            await FillIndexLookupsAsync(vm, ct);

            if (vm.HallId.HasValue)
            {
                vm.Templates = await _service.GetListForHallAsync(vm.HallId.Value, ct);
            }
            else
            {
                vm.Templates = new List<PricingTemplateListItemDto>();
            }

            return View(IndexViewPath, vm);
        }

        // GET: /Admin/PricingTemplates/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? cinemaId, int? hallId, string? returnUrl, CancellationToken ct)
        {
            var vm = new PricingTemplateEditVm
            {
                IsActive = true,
                CinemaId = cinemaId,
                HallId = cinemaId.HasValue ? hallId : null
            };

            await FillEditLookupsAsync(vm, ct);

            if (vm.HallId.HasValue && !vm.Halls.Any(h => h.Value == vm.HallId.Value.ToString()))
                vm.HallId = null;

            ViewBag.ReturnUrl = returnUrl;
            return View(CreateViewPath, vm);
        }

        // POST: /Admin/PricingTemplates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PricingTemplateEditVm vm, string? returnUrl, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await FillEditLookupsAsync(vm, ct);
                ViewBag.ReturnUrl = returnUrl;
                return View(CreateViewPath, vm);
            }

            try
            {
                var dto = new PricingTemplateEditDto
                {
                    Name = vm.Name,
                    IsActive = vm.IsActive,
                    HallId = vm.HallId,
                    RowPrices = vm.RowPrices,
                    CategoryMultipliers = vm.CategoryMultipliers
                };

                await _service.CreateAsync(dto, ct);

                TempData["Success"] = "Шаблон успішно створено.";

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                return RedirectToAction(nameof(Index), new { CinemaId = vm.CinemaId, HallId = vm.HallId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await FillEditLookupsAsync(vm, ct);
                ViewBag.ReturnUrl = returnUrl;
                return View(CreateViewPath, vm);
            }
        }

        // GET: /Admin/PricingTemplates/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id, string? returnUrl, CancellationToken ct)
        {
            var dto = await _service.GetForEditAsync(id, ct);
            if (dto == null) return NotFound();

            var vm = new PricingTemplateEditVm
            {
                Id = dto.Id,
                Name = dto.Name,
                IsActive = dto.IsActive,
                HallId = dto.HallId,
                RowPrices = dto.RowPrices,
                CategoryMultipliers = dto.CategoryMultipliers
            };

            if (vm.HallId.HasValue)
            {
                var allHalls = await _lookups.GetHallsAsync(ct);
                var currentHall = allHalls.FirstOrDefault(h => h.Id == vm.HallId.Value);

            }

            var cinemas = await _lookups.GetCinemasAsync(ct);
            vm.Cinemas = ToSelectList(cinemas, null);

            var allPossibleHalls = await _lookups.GetHallsAsync(ct);
            vm.Halls = ToSelectList(allPossibleHalls, vm.HallId);

            ViewBag.ReturnUrl = returnUrl;
            return View(EditViewPath, vm);
        }

        // POST: /Admin/PricingTemplates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PricingTemplateEditVm vm, string? returnUrl, CancellationToken ct)
        {
            if (id != vm.Id) vm.Id = id;

            if (!ModelState.IsValid)
            {
                await FillEditLookupsAsync(vm, ct);

                ViewBag.ReturnUrl = returnUrl;
                return View(EditViewPath, vm);
            }

            try
            {
                var dto = new PricingTemplateEditDto
                {
                    Id = id,
                    Name = vm.Name,
                    IsActive = vm.IsActive,
                    HallId = vm.HallId,
                    RowPrices = vm.RowPrices,
                    CategoryMultipliers = vm.CategoryMultipliers
                };

                await _service.UpdateAsync(dto, ct);

                TempData["Success"] = "Шаблон успішно оновлено.";

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction(nameof(Index), new { CinemaId = vm.CinemaId, HallId = vm.HallId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await FillEditLookupsAsync(vm, ct);
                ViewBag.ReturnUrl = returnUrl;
                return View(EditViewPath, vm);
            }
        }

        // POST: /Admin/PricingTemplates/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            TempData["Success"] = "Шаблон видалено.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/PricingTemplates/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, CancellationToken ct)
        {
            try
            {
                await _service.ToggleStatusAsync(id, ct);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}