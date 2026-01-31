using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Movies;

namespace Web.Controllers;

public class MoviesController : Controller
{
    private readonly IMoviePublicService _moviePublicService;

    public MoviesController(IMoviePublicService moviePublicService)
    {
        _moviePublicService = moviePublicService;
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var dto = await _moviePublicService.GetDetailsAsync(id, ct);
        if (dto is null) return NotFound();

        var vm = new MovieDetailsVm
        {
            Id = dto.Id,
            Title = dto.Title,
            ReleaseDate = dto.ReleaseDate,
            OriginalName = dto.OriginalName,
            DirectorName = dto.DirectorName,
            Description = dto.Description,
            Language = dto.Language,
            Duration = dto.Duration,
            Country = dto.Country,
            TrailerUrl = dto.TrailerUrl,
            Rating = dto.Rating,
            PosterUrl = string.IsNullOrWhiteSpace(dto.PosterUrl) ? "/images/no-poster.png" : dto.PosterUrl
        };

        return View(vm);
    }
}
