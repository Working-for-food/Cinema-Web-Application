using System;

namespace Infrastructure.Entities;

public class Cinema
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? City { get; set; }
    public ICollection<Hall> Halls { get; set; } = new List<Hall>();
}
