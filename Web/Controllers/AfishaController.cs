using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Afisha;

namespace Web.Controllers;

public class AfishaController : Controller
{
    private readonly IAfishaService _afishaService;

    public AfishaController(IAfishaService afishaService)
    {
        _afishaService = afishaService;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var dto = await _afishaService.GetAfishaAsync(ct);

        var vm = new AfishaIndexVm
        {
            NowShowing = dto.NowShowing.Select(x => new MovieCardVm
            {
                Id = x.Id,
                Title = x.Title,
                PosterUrl = string.IsNullOrWhiteSpace(x.PosterUrl) ? "/images/no-poster.png" : x.PosterUrl,
                Year = x.Year,
                AgeLabel = x.AgeLabel
            }).ToList(),
            ComingSoon = dto.ComingSoon.Select(x => new MovieCardVm
            {
                Id = x.Id,
                Title = x.Title,
                PosterUrl = string.IsNullOrWhiteSpace(x.PosterUrl) ? "/images/no-poster.png" : x.PosterUrl,
                Year = x.Year,
                AgeLabel = x.AgeLabel
            }).ToList()
        };

        return View(vm);
    }
}
