namespace Web.ViewModels.Afisha;

public class MovieCardVm
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string PosterUrl { get; set; } = "";
    public int? Year { get; set; }
    public string AgeLabel { get; set; } = "";
}
