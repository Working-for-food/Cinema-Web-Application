using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Infrastructure.Entities;
using Application.Interfaces;

namespace Web.Controllers.Admin
{
    [Route("admin/movies")]
    public class AdminMoviesController : Controller
    {
        private readonly CinemaDbContext _context;
        private readonly ITmdbClient _tmdb;
        private readonly IImportMovieFromTmdb _import;
        public AdminMoviesController(CinemaDbContext context, ITmdbClient tmdb, IImportMovieFromTmdb import)
        {
            _context = context;
            _tmdb = tmdb;
            _import = import;
        }

        // GET: Movies
        public async Task<IActionResult> Index(bool showDeleted, CancellationToken ct)
        {
            var q = _context.Movies.AsNoTracking();

            if (!showDeleted)
                q = q.Where(m => !m.IsDeleted);

            var movies = await q
                .OrderByDescending(m => m.ReleaseDate)
                .ToListAsync(ct);

            ViewBag.ShowDeleted = showDeleted;
            return View(movies);
        }
        [HttpGet("add")]
        public IActionResult Add() => View();
        [HttpGet("add/search")]
        public async Task<IActionResult> Search([FromQuery] string query, CancellationToken ct)
        {
            var items = await _tmdb.SearchMovieAsync(query, page: 1, ct: ct);

            var sorted = items
                .Select(x =>
                {
                    DateTime? dt = null;
                    if (!string.IsNullOrWhiteSpace(x.ReleaseDate) &&
                        DateTime.TryParse(x.ReleaseDate, out var parsed))
                        dt = parsed;

                    return new
                    {
                        tmdbId = x.Id,
                        title = x.Title,
                        releaseDate = x.ReleaseDate,
                        posterPath = x.PosterPath,
                        releaseDateParsed = dt
                    };
                })
                .OrderByDescending(x => x.releaseDateParsed ?? DateTime.MinValue)
                .Select(x => new
                {
                    x.tmdbId,
                    x.title,
                    x.releaseDate,
                    x.posterPath
                });

            return Ok(sorted);
        }
        public sealed record ImportRequest(int TmdbId);

        [HttpPost("add/import")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import([FromForm] int tmdbId, CancellationToken ct)
        {
            var movieId = await _import.ImportAsync(tmdbId, ct);
            return RedirectToAction(nameof(Edit), new { id = movieId });
        }
        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id, ct);
            if (movie is null) return NotFound();

            return View(nameof(Edit), movie);
        }

        // POST /movies/edit/5
        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSave(int id, [FromForm] Movie input, CancellationToken ct)
        {
            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id, ct);
            if (movie is null) return NotFound();

            movie.Title = input.Title;
            movie.OriginalTitle = input.OriginalTitle;
            movie.Description = input.Description;
            movie.ReleaseDate = input.ReleaseDate;
            movie.Duration = input.Duration;
            movie.Language = input.Language;
            movie.TrailerUrl = input.TrailerUrl;
            movie.Rating = input.Rating;
            movie.PosterPath = input.PosterPath;
            movie.BackdropPath = input.BackdropPath;

            await _context.SaveChangesAsync(ct);
            return RedirectToAction(nameof(Index));
        }

        // GET /admin/movies/delete/5
        [HttpGet("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var movie = await _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, ct);
            if (movie is null) return NotFound();
            return View(movie);
        }

        // POST /admin/movies/delete/5
        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id, ct);

            if (movie is null) return NotFound();

            movie.IsDeleted = true;
            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Index));
        }
    }
}
