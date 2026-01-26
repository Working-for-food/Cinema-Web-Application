using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Interfaces;

namespace Application.Services;

public class SessionLookupService : ISessionLookupService
{
    private readonly ITestMovieRepository _movies;
    private readonly ITestHallRepository _halls;

    public SessionLookupService(ITestMovieRepository movies, ITestHallRepository halls)
    {
        _movies = movies;
        _halls = halls;
    }

    public async Task<List<LookupItemDto>> GetMoviesAsync(string? query, CancellationToken ct)
    {
        var list = await _movies.SearchAsync(query, take: 50, ct);
        return list.Select(m => new LookupItemDto(m.Id, m.Title)).ToList();
    }

    public async Task<List<LookupItemDto>> GetHallsAsync(CancellationToken ct)
    {
        var list = await _halls.GetAllWithCinemaAsync(ct);
        return list.Select(h => new LookupItemDto(h.Id, $"{h.Cinema.Name} — {h.Name}")).ToList();
    }
}
