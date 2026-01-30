using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Entities;

public class Movie
{
    public int Id { get; set; }
    public int? TmdbId { get; set; }
    public string? PosterPath { get; set; }
    public string? BackdropPath { get; set; }
    public string Title { get; set; } = null!;
    public DateOnly? ReleaseDate { get; set; }
    public string? OriginalTitle { get; set; }
    public string? Description { get; set; }
    public string? Language { get; set; }
    public int? Duration { get; set; }
    public string? TrailerUrl { get; set; }
    public decimal? Rating { get; set; } // decimal(4,1)
    public bool IsDeleted { get; set; } = false;
    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    public ICollection<MovieDirector> MovieDirectors { get; set; } = new List<MovieDirector>();
    public ICollection<MovieCountry> MovieCountries { get; set; } = new List<MovieCountry>();
    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
