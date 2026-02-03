using Application.DTOs.Movies;
using Application.Interfaces;
using Infrastructure.Interfaces;

namespace Application.Services;

public class MoviePublicService : IMoviePublicService
{
    private const string TmdbBase = "https://image.tmdb.org/t/p/w500";
    private readonly IUserMovieRepository _movieRepository;

    public MoviePublicService(IUserMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<MovieDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        var movie = await _movieRepository.GetByIdAsync(id, ct);
        if (movie is null) return null;

        string posterUrl = string.IsNullOrWhiteSpace(movie.PosterPath) ? "" : $"{TmdbBase}{movie.PosterPath}";

        var directors = movie.MovieDirectors
            .Where(x => x.Director is not null)
            .Select(x => x.Director!.FullName)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();
        var countries = movie.MovieCountries
           .Where(x => x.Country is not null)
           .Select(x => x.Country!.Name)
           .Where(s => !string.IsNullOrWhiteSpace(s))
           .Distinct()
           .ToList();
        return new MovieDetailsDto
        {
            Id = movie.Id,
            Title = movie.Title,
            ReleaseDate = movie.ReleaseDate,
            OriginalName = movie.OriginalName,
            Description = movie.Description,
            Language = movie.Language,
            Duration = movie.Duration,
            TrailerUrl = movie.TrailerUrl,
            Rating = movie.Rating,
            PosterUrl = posterUrl,
            Directors = directors,
            Countries = countries
        };
    }
}
