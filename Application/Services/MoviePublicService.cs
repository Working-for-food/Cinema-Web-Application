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

        return new MovieDetailsDto
        {
            Id = movie.Id,
            Title = movie.Title,
            ReleaseDate = movie.ReleaseDate,
            OriginalName = movie.OriginalName,
            DirectorName = movie.Director is null ? null : $"{movie.Director.FirstName} {movie.Director.LastName}",
            Description = movie.Description,
            Language = movie.Language,
            Duration = movie.Duration,
            Country = movie.ProductionCountry?.Name,
            TrailerUrl = movie.TrailerUrl,
            Rating = movie.Rating,
            PosterUrl = posterUrl
        };
    }
}
