namespace Infrastructure.Entities;

public class MovieCountry
{
    public int MovieId { get; set; }
    public string CountryCode { get; set; } = null!; // char(2)

    public Movie Movie { get; set; } = null!;
    public Country Country { get; set; } = null!;
}
