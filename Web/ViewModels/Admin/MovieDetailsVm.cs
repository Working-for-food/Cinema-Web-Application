using Infrastructure.Entities;

namespace Web.ViewModels.Admin;

public sealed class MovieDetailsVm
{
    public Movie Movie { get; set; } = null!;
    public string Metacritic { get; set; } = "—";
}
