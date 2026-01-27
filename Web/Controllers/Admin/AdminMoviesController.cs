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
        public AdminMoviesController(CinemaDbContext context, ITmdbClient tmdb)
        {
            _context = context;
            _tmdb = tmdb;
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
            var existing = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId, ct);
            var details = await _tmdb.GetMovieDetailsAsync(tmdbId, ct);
            var credits = await _tmdb.GetCreditsAsync(tmdbId, ct);
            var videos = await _tmdb.GetVideosAsync(tmdbId, ct);

            var trailer = videos.Results
                .FirstOrDefault(v => v.Site == "YouTube" && v.Type == "Trailer" && !string.IsNullOrWhiteSpace(v.Key));
            var trailerUrl = trailer is null ? null : $"https://www.youtube.com/watch?v={trailer.Key}";

            DateOnly? releaseDate = null;
            if (!string.IsNullOrWhiteSpace(details.ReleaseDate) &&
                DateOnly.TryParse(details.ReleaseDate, out var parsed))
                releaseDate = parsed;
           
            Movie movie;
            if (existing is null)
            {
                movie = new Movie
                {
                    TmdbId = details.Id
                };
                _context.Movies.Add(movie);
            }
            else
            {
                movie = existing;
                if (movie.IsDeleted)
                    movie.IsDeleted = false;
            }
            movie.Title = details.Title ?? "(no title)";
            movie.OriginalTitle = details.OriginalTitle;
            movie.Description = details.Overview;
            movie.ReleaseDate = releaseDate;
            movie.Duration = details.Runtime;
            movie.PosterPath = details.PosterPath;
            movie.BackdropPath = details.BackdropPath;
            movie.Rating = details.VoteAverage;
            movie.TrailerUrl = trailerUrl;

            await _context.SaveChangesAsync(ct);

            // 1) Genres -> Genre + MovieGenre
            if (details.Genres is not null && details.Genres.Count > 0)
            {
                var tmdbGenreIds = details.Genres.Select(g => g.Id).Distinct().ToArray();

                var existingGenres = await _context.Genres
                    .Where(g => g.TmdbId != null && tmdbGenreIds.Contains(g.TmdbId.Value))
                    .ToListAsync(ct);

                var genreByTmdbId = existingGenres.ToDictionary(g => g.TmdbId!.Value);

                foreach (var g in details.Genres)
                {
                    if (!genreByTmdbId.TryGetValue(g.Id, out var genre))
                    {
                        genre = new Genre { TmdbId = g.Id, Name = g.Name ?? "—" };
                        _context.Genres.Add(genre);
                        genreByTmdbId[g.Id] = genre;
                    }
                    else if (!string.IsNullOrWhiteSpace(g.Name))
                    {
                        genre.Name = g.Name!;
                    }
                }

                await _context.SaveChangesAsync(ct);

                var oldLinks = await _context.MovieGenres
                    .Where(x => x.MovieId == movie.Id)
                    .ToListAsync(ct);

                _context.MovieGenres.RemoveRange(oldLinks);

                _context.MovieGenres.AddRange(details.Genres.Select(g => new MovieGenre
                {
                    MovieId = movie.Id,
                    GenreId = genreByTmdbId[g.Id].Id
                }));
            }

            // 2) Countries -> MovieCountry (тільки якщо є в seeded Countries)
            if (details.ProductionCountries is not null && details.ProductionCountries.Count > 0)
            {
                var codes = details.ProductionCountries
                    .Select(c => (c.Iso3166_1 ?? "").Trim().ToUpperInvariant())
                    .Where(code => code.Length == 2)
                    .Distinct()
                    .ToArray();

                var existingCodes = await _context.Countries
                    .Where(c => codes.Contains(c.Code))
                    .Select(c => c.Code)
                    .ToListAsync(ct);

                var oldLinks = await _context.MovieCountries
                    .Where(x => x.MovieId == movie.Id)
                    .ToListAsync(ct);

                _context.MovieCountries.RemoveRange(oldLinks);

                _context.MovieCountries.AddRange(existingCodes.Select(code => new MovieCountry
                {
                    MovieId = movie.Id,
                    CountryCode = code
                }));
            }

            // helper: upsert Person by tmdb id (FullName required)
            async Task<Person> UpsertPersonAsync(int personTmdbId, string? fullName, string? profilePath)
            {
                var person = await _context.People.FirstOrDefaultAsync(p => p.TmdbId == personTmdbId, ct);
                if (person is null)
                {
                    person = new Person
                    {
                        TmdbId = personTmdbId,
                        FullName = string.IsNullOrWhiteSpace(fullName) ? "Unknown" : fullName.Trim(),
                        PhotoUrl = profilePath,
                        TmdbLastSyncAt = DateTimeOffset.UtcNow
                    };
                    _context.People.Add(person);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(fullName))
                        person.FullName = fullName.Trim();
                    if (!string.IsNullOrWhiteSpace(profilePath))
                        person.PhotoUrl = profilePath;
                    person.TmdbLastSyncAt = DateTimeOffset.UtcNow;
                }

                return person;
            }

            // 3) Cast -> People + MovieActor (Top 7)
            if (credits.Cast is not null && credits.Cast.Count > 0)
            {
                var cast = credits.Cast
                    .OrderBy(c => c.Order)
                    .Take(7)
                    .ToList();

                var oldLinks = await _context.MovieActors
                    .Where(x => x.MovieId == movie.Id)
                    .ToListAsync(ct);

                _context.MovieActors.RemoveRange(oldLinks);

                foreach (var c in cast)
                {
                    var person = await UpsertPersonAsync(c.Id, c.Name, c.ProfilePath);

                    _context.MovieActors.Add(new MovieActor
                    {
                        Movie = movie,
                        Actor = person,
                        CustOrder = (short)Math.Clamp(c.Order, 0, short.MaxValue),
                        CharacterName = c.Character
                    });
                }
            }
            // 4) Directors -> People + MovieDirector
            if (credits.Crew is not null && credits.Crew.Count > 0)
            {
                var directors = credits.Crew
                    .Where(x => string.Equals(x.Job, "Director", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var oldLinks = await _context.MovieDirectors
                    .Where(x => x.MovieId == movie.Id)
                    .ToListAsync(ct);

                _context.MovieDirectors.RemoveRange(oldLinks);

                for (var i = 0; i < directors.Count; i++)
                {
                    var d = directors[i];
                    var person = await UpsertPersonAsync(d.Id, d.Name, d.ProfilePath);

                    _context.MovieDirectors.Add(new MovieDirector
                    {
                        Movie = movie,
                        Director = person,
                        BillingOrder = (short)Math.Clamp(i, 0, short.MaxValue)
                    });
                }
            }

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Edit), new { id = movie.Id });
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
