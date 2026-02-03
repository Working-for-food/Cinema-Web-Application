using Application.DTOs.TmdbDtos;

namespace Application.Services;

public static class TmdbAgeRatingHelper
{
    public static int? GetAgeRating(TmdbMovieReleaseDatesResponse? releaseDates)
    {
        if (releaseDates?.Results is null || releaseDates.Results.Count == 0) return null;

        string? cert =
            GetFirstNonEmptyCert(releaseDates, "UA") ??
            GetFirstNonEmptyCert(releaseDates, "US") ??
            GetFirstNonEmptyCertAnyCountry(releaseDates);

        return ParseCertificationToAge(cert);
    }

    private static string? GetFirstNonEmptyCert(TmdbMovieReleaseDatesResponse r, string countryCode)
        => r.Results
            .FirstOrDefault(x => string.Equals(x.Iso3166_1, countryCode, StringComparison.OrdinalIgnoreCase))
            ?.ReleaseDates
            .Select(x => x.Certification?.Trim())
            .FirstOrDefault(c => !string.IsNullOrWhiteSpace(c));

    private static string? GetFirstNonEmptyCertAnyCountry(TmdbMovieReleaseDatesResponse r)
        => r.Results
            .SelectMany(x => x.ReleaseDates)
            .Select(x => x.Certification?.Trim())
            .FirstOrDefault(c => !string.IsNullOrWhiteSpace(c));

    private static int? ParseLeadingInt(string? cert)
    {
        if (string.IsNullOrWhiteSpace(cert)) return null;

        int value = 0;
        bool hasDigit = false;

        foreach (var ch in cert)
        {
            if (char.IsDigit(ch))
            {
                hasDigit = true;
                value = checked(value * 10 + (ch - '0'));
            }
            else if (hasDigit)
            {
                break;
            }
        }

        return hasDigit ? value : null;
    }
    private static int? ParseCertificationToAge(string? cert)
    {
        if (string.IsNullOrWhiteSpace(cert)) return null;

        cert = cert.Trim().ToUpperInvariant();

        return cert switch
        {
            "G" => 0,
            "PG" => 6,
            "PG-13" => 12,
            "R" => 16,
            "NC-17" => 18,
            _ => ParseLeadingInt(cert)
        };
    }
    public static string? FormatPlus(int? age) => age is null ? null : $"{age}+";
}
