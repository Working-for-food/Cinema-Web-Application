using Application.DTOs.Afisha;
using Application.Interfaces;
using Infrastructure.Interfaces;

namespace Application.Services;

public class AfishaService : IAfishaService
{
    private const string TmdbBase = "https://image.tmdb.org/t/p/w500";
    private readonly IAfishaRepository _afishaRepository;

    public AfishaService(IAfishaRepository afishaRepository)
    {
        _afishaRepository = afishaRepository;
    }

    public async Task<AfishaIndexDto> GetAfishaAsync(CancellationToken ct = default)
    {
        var now = await _afishaRepository.GetNowShowingAsync(ct);
        var soon = await _afishaRepository.GetComingSoonAsync(ct);

        string BuildPosterUrl(string? posterPath)
            => string.IsNullOrWhiteSpace(posterPath) ? "" : $"{TmdbBase}{posterPath}";

        return new AfishaIndexDto
        {
            NowShowing = now.Select(m => new MovieCardDto
            {
                Id = m.Id,
                Title = m.Title,
                PosterUrl = BuildPosterUrl(m.PosterPath)
            }).ToList(),
            ComingSoon = soon.Select(m => new MovieCardDto
            {
                Id = m.Id,
                Title = m.Title,
                PosterUrl = BuildPosterUrl(m.PosterPath)
            }).ToList()
        };
    }
}
