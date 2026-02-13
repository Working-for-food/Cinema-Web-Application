namespace Web.Helpers;

public static class TmdbImage
{
    private const string BaseUrl = "https://image.tmdb.org/t/p/";

    // Common TMDB sizes (defaults for your project)
    public const string DefaultProfileSize = "w185";
    public const string DefaultPosterSize = "w342";
    public const string DefaultBackdropSize = "w780";

    public static string? Profile(string? path, string size = DefaultProfileSize)
        => Build(path, size);

    public static string? Poster(string? path, string size = DefaultPosterSize)
        => Build(path, size);

    public static string? Backdrop(string? path, string size = DefaultBackdropSize)
        => Build(path, size);

    public static string? Build(string? path, string size)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        path = path.Trim();

        // already absolute
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return path;

        // tmdb relative like "/abc.jpg"
        if (path.StartsWith('/'))
            return $"{BaseUrl}{size}{path}";

        // unknown format - leave as-is
        return path;
    }

    public static string OrFallback(string? url, string fallback)
        => string.IsNullOrWhiteSpace(url) ? fallback : url;
}
