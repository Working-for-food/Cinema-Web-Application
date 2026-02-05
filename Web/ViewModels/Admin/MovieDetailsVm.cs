using Infrastructure.Entities;

namespace Web.ViewModels.Admin;

public sealed class MovieDetailsVm
{
    public Movie Movie { get; set; } = null!;
    public string RottenTomatoes { get; set; } = "Ще не оцінено критиками";
    public string ImdbRating { get; set; } = "Ще не оцінено глядачами";
    public string Metacritic { get; set; } = "Ще не оцінено критиками";
}
