using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers;

[Authorize]
[Route("saved")]
public class SavedMoviesController : Controller
{
    private readonly CinemaDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public SavedMoviesController(CinemaDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpPost("toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int movieId, string? returnUrl = null, CancellationToken ct = default)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var existing = await _db.SavedMovies
            .FirstOrDefaultAsync(x => x.UserId == userId && x.MovieId == movieId, ct);

        if (existing is null)
            _db.SavedMovies.Add(new SavedMovie
            {
                UserId = userId,
                MovieId = movieId,
                CreatedAtUtc = DateTime.UtcNow
            });
        else
            _db.SavedMovies.Remove(existing);

        await _db.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("My");
    }

    [HttpGet("my")]
    public async Task<IActionResult> My(CancellationToken ct = default)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var items = await _db.SavedMovies
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => x.Movie)
            .AsNoTracking()
            .ToListAsync(ct);

        return View(items);
    }
}
