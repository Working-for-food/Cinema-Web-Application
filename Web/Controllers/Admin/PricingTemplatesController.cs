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

        // GET: /Admin/PricingTemplates/Index
        [HttpGet]
        public async Task<IActionResult> Index(int? hallId, CancellationToken ct)
        {
            var halls = await _lookups.GetHallsAsync(ct);

            ViewBag.Halls = halls.Select(h => new SelectListItem(h.Title, h.Id.ToString(), h.Id == hallId));

            var list = hallId.HasValue
                ? await _service.GetListForHallAsync(hallId.Value, ct)
                : new List<PricingTemplateListItemDto>();

            return View(IndexViewPath, list);
        }

        // GET: /Admin/PricingTemplates/Create
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var halls = await _lookups.GetHallsAsync(ct);

            var vm = new PricingTemplateEditVm
            {
                IsActive = true
            };

            ViewBag.Halls = halls.Select(h => new SelectListItem(h.Title, h.Id.ToString()));

            return View(CreateViewPath, vm);
        }

        // POST: /Admin/PricingTemplates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PricingTemplateEditVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await ReloadLookupsAsync(ct);
                return View(CreateViewPath, vm);
            }

            try
            {
                var dto = new PricingTemplateEditDto
                {
                    Id = vm.Id,
                    Name = vm.Name,
                    IsActive = vm.IsActive,
                    HallId = vm.HallId,
                    RowPrices = vm.RowPrices,
                    CategoryMultipliers = vm.CategoryMultipliers
                };

                await _service.CreateAsync(dto, ct);

                TempData["Success"] = "Шаблон успішно створено.";
                return RedirectToAction(nameof(Index), new { hallId = vm.HallId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await ReloadLookupsAsync(ct);
                return View(CreateViewPath, vm);
            }
        }

        // GET: /Admin/PricingTemplates/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var dto = await _service.GetForEditAsync(id, ct);
            if (dto == null) return NotFound();

            var halls = await _lookups.GetHallsAsync(ct);

            ViewBag.Halls = halls.Select(h => new SelectListItem(h.Title, h.Id.ToString()));

            var vm = new PricingTemplateEditVm
            {
                Id = dto.Id,
                Name = dto.Name,
                IsActive = dto.IsActive,
                HallId = dto.HallId,

                RowPrices = dto.RowPrices,
                CategoryMultipliers = dto.CategoryMultipliers
            };

            return View(EditViewPath, vm);
        }

        // POST: /Admin/PricingTemplates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PricingTemplateEditVm vm, CancellationToken ct)
        {

            if (!ModelState.IsValid)
            {
                await ReloadLookupsAsync(ct);

                TempData["Error"] = "Перевірте правильність введених даних.";

                return View("~/Views/Admin/PricingTemplates/Edit.cshtml", vm);
            }

            try
            {
                var dto = new PricingTemplateEditDto
                {
                    Id = vm.Id,
                    Name = vm.Name,
                    IsActive = vm.IsActive,
                    HallId = vm.HallId,
                    RowPrices = vm.RowPrices,
                    CategoryMultipliers = vm.CategoryMultipliers
                };

                await _service.UpdateAsync(dto, ct);

                TempData["Success"] = "Шаблон успішно оновлено.";
                return RedirectToAction(nameof(Index), new { hallId = vm.HallId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await ReloadLookupsAsync(ct);
                return View(EditViewPath, vm);
            }
        }

        // POST: /Admin/PricingTemplates/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            int? returnHallId = null;
            var dto = await _service.GetForEditAsync(id, ct);

            if (dto != null)
            {
                returnHallId = dto.HallId;
            }

            try
            {
                await _service.DeleteAsync(id, ct);
                TempData["Success"] = "Шаблон успішно видалено.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Помилка видалення: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { hallId = returnHallId });
        }

        // POST: /Admin/PricingTemplates/ToggleStatus?id=5
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

        private async Task ReloadLookupsAsync(CancellationToken ct)
        {
            var halls = await _lookups.GetHallsAsync(ct);
            ViewBag.Halls = halls.Select(h => new SelectListItem(h.Title, h.Id.ToString()));
        }
    }
}