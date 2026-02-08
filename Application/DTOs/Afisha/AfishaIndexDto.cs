namespace Application.DTOs.Afisha;

public class AfishaIndexDto
{
    public List<MovieCardDto> NowShowing { get; set; } = [];
    public List<MovieCardDto> ComingSoon { get; set; } = [];
}
