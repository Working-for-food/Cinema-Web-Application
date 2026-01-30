using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Movies;

namespace Web.Controllers;


public class MoviesController : Controller
{
    private readonly IUserMovieRepository _movieRepository;

    public MoviesController(IUserMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var movie = await _movieRepository.GetByIdAsync(id, ct);
        if (movie is null) return NotFound();

        var vm = new MovieDetailsVm
        {
            Id = movie.Id,
            Title = movie.Title,
            ReleaseDate = movie.ReleaseDate,
            OriginalName = movie.OriginalName,
            DirectorName = movie.Director is null ? null : $"{movie.Director.FirstName} {movie.Director.LastName}",
            Description = movie.Description,
            Language = movie.Language,
            Duration = movie.Duration,
            Country = movie.ProductionCountry?.Name,
            TrailerUrl = movie.TrailerUrl,
            Rating = movie.Rating,
            PosterUrl = $"/posters/{movie.Id}.jpg"
        };

        return View(vm);
    }
}
