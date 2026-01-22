using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.ViewModels.Admin;

namespace Web.Controllers.Admin
{
    [Route("admin/movies")]
    public class MoviesController : Controller
    {
        private readonly MovieService _service;

        public MoviesController(MovieService service)
        {
            _service = service;
        }

        // GET: Movies
        [HttpGet("")]
        public async Task<IActionResult> Index(string search, string sortBy = "title", int page = 1)
        {
            var (movies, totalCount) = await _service.GetMoviesAsync(search, sortBy, page);

            var vm = new MovieIndexVm
            {
                Movies = movies,
                Page = page,
                TotalPages = (int)Math.Ceiling(totalCount / 10.0),
                SearchTerm = search,
                SortBy = sortBy
            };

            return View("~/Views/Admin/Movies/Index.cshtml", vm);
        }

        // GET: Movies/Create
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var genres = await _service.GetGenresAsync();
            var vm = new MovieEditVm
            {
                // Підготовка списку жанрів для View
                GenreList = new MultiSelectList(genres, "Id", "Name")
            };
            return View("~/Views/Admin/Movies/Create.cshtml", vm);
        }

        // POST: Movies/Create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieEditVm model)
        {
            if (!ModelState.IsValid)
            {
                // Перезавантажуємо список жанрів при помилці
                var genres = await _service.GetGenresAsync();
                model.GenreList = new MultiSelectList(genres, "Id", "Name", model.SelectedGenreIds);
                return View("~/Views/Admin/Movies/Create.cshtml", model);
            }

            try 
            {
                var dto = new MovieFormDto
                {
                    Title = model.Title,
                    Description = model.Description,
                    Duration = model.Duration,
                    ReleaseDate = model.ReleaseDate,
                    ProductionCountryCode = model.ProductionCountryCode,
                    GenreIds = model.SelectedGenreIds
                };

                await _service.CreateMovieAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var genres = await _service.GetGenresAsync();
                model.GenreList = new MultiSelectList(genres, "Id", "Name", model.SelectedGenreIds);
                return View("~/Views/Admin/Movies/Create.cshtml", model);
            }
        }

        // GET: Movies/Edit/5
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try 
            {
                var dto = await _service.GetMovieForEditAsync(id);
                var genres = await _service.GetGenresAsync();

                var vm = new MovieEditVm
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    Description = dto.Description,
                    Duration = dto.Duration,
                    ReleaseDate = dto.ReleaseDate,
                    ProductionCountryCode = dto.ProductionCountryCode,
                    SelectedGenreIds = dto.GenreIds,
                    GenreList = new MultiSelectList(genres, "Id", "Name", dto.GenreIds)
                };
                return View("~/Views/Admin/Movies/Edit.cshtml", vm);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Movies/Edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovieEditVm model)
        {
            if (!ModelState.IsValid)
            {
                var genres = await _service.GetGenresAsync();
                model.GenreList = new MultiSelectList(genres, "Id", "Name", model.SelectedGenreIds);
                return View("~/Views/Admin/Movies/Edit.cshtml", model);
            }

            try
            {
                var dto = new MovieFormDto
                {
                    Id = model.Id,
                    Title = model.Title,
                    Description = model.Description,
                    Duration = model.Duration,
                    ReleaseDate = model.ReleaseDate,
                    ProductionCountryCode = model.ProductionCountryCode,
                    GenreIds = model.SelectedGenreIds
                };

                await _service.UpdateMovieAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Admin/Movies/Edit.cshtml", model);
            }
        }
        
        // POST: Movies/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteMovieAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}