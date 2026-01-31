namespace Application.DTOs.Afisha;

public class MovieCardDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string PosterUrl { get; set; } = "";
    public int? Year { get; set; }
    public string AgeLabel { get; set; } = "";

}
