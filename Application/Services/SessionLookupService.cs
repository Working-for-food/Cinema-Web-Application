using Application.DTOs;
using Application.DTOs.Pricing;
using Application.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Application.Services;

public class SessionLookupService : ISessionLookupService
{
    private readonly IMovieRepository _movies;
    private readonly IHallRepository _halls;
    private readonly ISeatRepository _seats;

    public SessionLookupService(IMovieRepository movies, IHallRepository halls, ISeatRepository seats)
    {
        _movies = movies;
        _halls = halls;
        _seats = seats;
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

    public async Task<List<SeatDto>> GetHallSeatsAsync(int hallId, CancellationToken ct)
    {
        if (hallId < 1)
            return new List<SeatDto>();

        ct.ThrowIfCancellationRequested();

        if (!await _halls.ExistsAsync(hallId))
            return new List<SeatDto>();

        ct.ThrowIfCancellationRequested();

        var seats = await _seats.GetByHallAsync(hallId);

        return seats
            .OrderBy(s => s.RowNumber)
            .ThenBy(s => s.SeatNumber)
            .Select(s => new SeatDto
            {
                Id = s.Id,
                RowNumber = s.RowNumber,
                SeatNumber = s.SeatNumber,
                Category = s.Category
            })
            .ToList();
    }


    public Task<string?> GetMovieTitleByIdAsync(int movieId, CancellationToken ct)
        => _movies.GetTitleByIdAsync(movieId, ct);

    public async Task<HallPricingMetaDto> GetHallPricingMetaAsync(int hallId, CancellationToken ct)
    {
        if (hallId < 1)
            return new HallPricingMetaDto(Array.Empty<int>(), Array.Empty<CategoryItemDto>());

        if (!await _halls.ExistsAsync(hallId))
            return new HallPricingMetaDto(Array.Empty<int>(), Array.Empty<CategoryItemDto>());

        var seats = await _seats.GetByHallAsync(hallId);

        var rows = seats
            .Select(s => s.RowNumber)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var categories = seats
            .Select(s => s.Category)
            .Distinct()
            .OrderBy(x => x)
            .Select(c => new CategoryItemDto((int)c, CategoryTitle(c)))
            .ToList();

        return new HallPricingMetaDto(rows, categories);
    }

    private static string CategoryTitle(SeatCategory c) => c switch
    {
        SeatCategory.Standard => "Звичайне",
        SeatCategory.Vip => "VIP",
        SeatCategory.Accessible => "Інклюзивне",
        _ => c.ToString()
    };
}