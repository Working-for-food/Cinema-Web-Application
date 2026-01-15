namespace Infrastructure.Entities;

public class Person
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;

    public string? FullName { get; set; }
    public DateOnly? BirthDate { get; set; }

    // FK -> Countries.code (optional)
    public string? CountryCode { get; set; }
    public string? PhotoUrl { get; set; }

    public Country? Country { get; set; }

    // navs
    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    public ICollection<MovieDirector> MovieDirectors { get; set; } = new List<MovieDirector>();

    // Movies.directorId
    public ICollection<Movie> DirectedMoviesMain { get; set; } = new List<Movie>();
}
