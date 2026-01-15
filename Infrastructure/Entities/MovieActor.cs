namespace Infrastructure.Entities;

public class MovieActor
{
    public int MovieId { get; set; }
    public int ActorId { get; set; }

    public short CustOrder { get; set; }
    public string? CharacterName { get; set; }

    public Movie Movie { get; set; } = null!;
    public Person Actor { get; set; } = null!;
}
