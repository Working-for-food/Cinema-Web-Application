namespace Application.Options;
public sealed class TmdbOptions
{
    public string BaseUrl { get; init; } = "https://api.themoviedb.org/3/";
    public string ApiKey { get; init; } = null!;
    public string Language { get; init; } = "uk-UA";
}
