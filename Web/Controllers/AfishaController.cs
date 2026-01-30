using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Afisha;

namespace Web.Controllers;


public class AfishaController : Controller
{
    private readonly IAfishaRepository _afishaRepository;

    public AfishaController(IAfishaRepository afishaRepository)
    {
        _afishaRepository = afishaRepository;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var now = await _afishaRepository.GetNowShowingAsync(ct);
        var soon = await _afishaRepository.GetComingSoonAsync(ct);

        var vm = new AfishaIndexVm
        {
            NowShowing = now.Select(m => new MovieCardVm
            {
                Id = m.Id,
                Title = m.Title,
                PosterUrl = $"/posters/{m.Id}.jpg"
            }).ToList(),
            ComingSoon = soon.Select(m => new MovieCardVm
            {
                Id = m.Id,
                Title = m.Title,
                PosterUrl = $"/posters/{m.Id}.jpg"
            }).ToList()
        };

        return View(vm);
    }
}
