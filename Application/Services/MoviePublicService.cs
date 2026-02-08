using Application.DTOs.Movies;
using Application.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Application.Services;

public class MoviePublicService : IMoviePublicService
{
    private const string TmdbBase = "https://image.tmdb.org/t/p/w500";
    private readonly IUserMovieRepository _movies;

    public MoviePublicService(IUserMovieRepository movies)
    {
        _movies = movies;
    }

    public async Task<MovieDetailsDto?> GetDetailsAsync(int movieId, DateOnly? date, CancellationToken ct = default)
    {
        var movie = await _movies.GetByIdWithDetailsAsync(movieId, ct);
        if (movie is null) return null;

        var baseDay = date ?? DateOnly.FromDateTime(DateTime.Now);
        var from = baseDay.ToDateTime(TimeOnly.MinValue);
        var to = date.HasValue ? from.AddDays(1) : from.AddDays(8);

        var sessions = movie.Sessions
            .Where(s => !s.IsCancelled && s.StartTime >= from && s.StartTime < to)
            .OrderBy(s => s.StartTime)
            .ToList();

        var schedule = sessions
            .GroupBy(s => new { CinemaId = s.Hall.Cinema.Id, CinemaName = s.Hall.Cinema.Name })
            .OrderBy(g => g.Key.CinemaName)
            .Select(cinemaGroup => new SessionsCinemaDto
            {
                CinemaId = cinemaGroup.Key.CinemaId,
                CinemaName = cinemaGroup.Key.CinemaName,
                Days = cinemaGroup
                    .GroupBy(s => DateOnly.FromDateTime(s.StartTime))
                    .OrderBy(g => g.Key)
                    .Select(dayGroup => new SessionsDayDto
                    {
                        Date = dayGroup.Key,
                        Slots = dayGroup
                            .OrderBy(s => s.StartTime)
                            .Select(s => new SessionSlotDto
                            {
                                Id = s.Id,
                                Start = TimeOnly.FromDateTime(s.StartTime),
                                End = TimeOnly.FromDateTime(s.EndTime),
                                HallName = s.Hall.Name,
                                Presentation = s.PresentationType switch
                                {
                                    PresentationType.TwoD => "2D",
                                    PresentationType.ThreeD => "3D",
                                    PresentationType.Imax => "IMAX",
                                    _ => s.PresentationType.ToString()
                                },
                                IsCancelled = s.IsCancelled
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToList();

        return new MovieDetailsDto
        {
            Id = movie.Id,
            Title = movie.Title,
            OriginalName = movie.OriginalName,
            Description = movie.Description,
            ReleaseDate = movie.ReleaseDate,
            Duration = movie.Duration,
            PosterPath = movie.PosterPath,
            BackdropPath = movie.BackdropPath,
            TrailerUrl = movie.TrailerUrl,
            Language = movie.Language,
            Rating = movie.Rating,

            Genres = movie.MovieGenres.Select(g => g.Genre.Name).ToList(),
            Countries = movie.MovieCountries.Select(c => c.Country.Name).ToList(),

            Actors = movie.MovieActors
                .OrderBy(a => a.CustOrder)
                .Select(a => new PersonShortDto
                {
                    Id = a.Actor.Id,
                    Name = a.Actor.FullName,
                    PhotoUrl = a.Actor.PhotoUrl
                })
                .ToList(),

            Directors = movie.MovieDirectors
                .OrderBy(d => d.BillingOrder)
                .Select(d => new PersonShortDto
                {
                    Id = d.Director.Id,
                    Name = d.Director.FullName,
                    PhotoUrl = d.Director.PhotoUrl
                })
                .ToList(),

            Schedule = schedule
        };
    }
}
