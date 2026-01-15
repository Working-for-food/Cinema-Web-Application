namespace Infrastructure.Entities;

public class MovieDirector
{
    public int MovieId { get; set; }
    public int DirectorId { get; set; }

    public short BillingOrder { get; set; }

    public Movie Movie { get; set; } = null!;
    public Person Director { get; set; } = null!;
}
