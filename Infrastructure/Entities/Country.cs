using System;

namespace Infrastructure.Entities;

public class Country
{
    // char(2) PK
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;

    // navs
    public ICollection<Person> People { get; set; } = new List<Person>();
    public ICollection<MovieCountry> MovieCountries { get; set; } = new List<MovieCountry>();
}
