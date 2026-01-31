namespace Infrastructure.Entities;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? TmdbId { get; set; }
    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
}
