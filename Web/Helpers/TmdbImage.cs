namespace Web.Helpers;

public static class TmdbImage
{
    private const string BaseUrl = "https://image.tmdb.org/t/p/";

    public static string? Build(string? path, string size)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        path = path.Trim();

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return path;

        if (path.StartsWith('/'))
            return $"{BaseUrl}{size}{path}";

        return path;
    }
}
