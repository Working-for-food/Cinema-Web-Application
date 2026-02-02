using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Interfaces;
using System.Linq;

namespace Application.Services;

public class SessionLookupService : ISessionLookupService
{
    private readonly IMovieRepository _movies;
    private readonly IHallRepository _halls;

    public SessionLookupService(IMovieRepository movies, IHallRepository halls)
    {
        _movies = movies;
        _halls = halls;
    }
    public async Task<List<LookupItemDto>> GetMoviesAsync(string? query, CancellationToken ct)
    {
        var list = await _movies.SearchAsync(query, take: 50, ct);

        return list
            .Select(m => new LookupItemDto(
                m.Id,
                m.Title,
                m.Duration,
                m.PosterPath
            ))
            .ToList();
    }

    public async Task<List<LookupItemDto>> GetHallsAsync(CancellationToken ct)
    {
        var list = await _halls.GetAllWithCinemaAsync();
        return list.Select(h => new LookupItemDto(h.Id, $"{h.Cinema.Name} — {h.Name}")).ToList();
    }

    public async Task<List<LookupItemDto>> GetCinemasAsync(CancellationToken ct)
    {
        var halls = await _halls.GetAllWithCinemaAsync();

        return halls
            .Where(h => h.Cinema != null)
            .GroupBy(h => h.Cinema.Id)
            .Select(g => g.First().Cinema)
            .OrderBy(c => c!.Name)
            .Select(c => new LookupItemDto(c!.Id, c!.Name))
            .ToList();
    }

    public async Task<List<LookupItemDto>> GetHallsByCinemaAsync(int cinemaId, CancellationToken ct)
    {
        var halls = await _halls.GetAllWithCinemaAsync();

        return halls
            .Where(h => h.CinemaId == cinemaId)
            .OrderBy(h => h.Name)
            .Select(h => new LookupItemDto(h.Id, h.Name))
            .ToList();
    }

    public async Task<string?> GetMovieTitleByIdAsync(int movieId, CancellationToken ct)
    {
        return await _movies.GetTitleByIdAsync(movieId, ct);
    }
}
