using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Interfaces;
using System.Linq;

namespace Application.Services;

public class SessionLookupService : ISessionLookupService
{
    private readonly ITestMovieRepository _movies;
    private readonly IHallRepository _halls;

    public SessionLookupService(ITestMovieRepository movies, IHallRepository halls)
    {
        _movies = movies;
        _halls = halls;
    }

    public async Task<List<LookupItemDto>> GetMoviesAsync(string? query, CancellationToken ct)
    {
        var list = await _movies.SearchAsync(query, take: 50, ct);
        return list.Select(m => new LookupItemDto(m.Id, m.Title, m.Duration)).ToList();
    }

    public async Task<List<LookupItemDto>> GetHallsAsync(CancellationToken ct)
    {
        var list = await _halls.GetAllWithCinemaAsync();
        return list.Select(h => new LookupItemDto(h.Id, $"{h.Cinema.Name} — {h.Name}")).ToList();
    }

    public async Task<string?> GetMovieTitleByIdAsync(int movieId, CancellationToken ct)
    {
        return await _movies.GetTitleByIdAsync(movieId, ct);
    }
}
