namespace Infrastructure.Entities;

public class Hall
{
    public int Id { get; set; }

    public int CinemaId { get; set; }
    public string Name { get; set; } = null!;

    public Cinema Cinema { get; set; } = null!;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
