using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Infrastructure.Data.Seed;

public static class CountryUkNameSeeder
{
    private sealed class CldrRoot
    {
        [JsonPropertyName("main")]
        public Dictionary<string, CldrLocale>? Main { get; set; }
    }

    private sealed class CldrLocale
    {
        [JsonPropertyName("localeDisplayNames")]
        public CldrLocaleDisplayNames? LocaleDisplayNames { get; set; }
    }

    private sealed class CldrLocaleDisplayNames
    {
        [JsonPropertyName("territories")]
        public Dictionary<string, string>? Territories { get; set; }
    }

    private static readonly Regex IsoA2 = new("^[A-Z]{2}$", RegexOptions.Compiled);

    public static async Task SeedAsync(CinemaDbContext db, CancellationToken ct = default)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "allcountryUK.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("allcountryUK.json not found", path);

        await using var stream = File.OpenRead(path);

        var root = await JsonSerializer.DeserializeAsync<CldrRoot>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            ct
        ) ?? throw new InvalidOperationException("allcountryUK.json is empty or invalid JSON.");

        if (root.Main is null || !root.Main.TryGetValue("uk", out var uk) ||
            uk.LocaleDisplayNames?.Territories is null)
        {
            throw new InvalidOperationException("allcountryUK.json does not contain main.uk.localeDisplayNames.territories");
        }

        var map = uk.LocaleDisplayNames.Territories
            .Select(kv => new
            {
                Code = (kv.Key ?? "").Trim().ToUpperInvariant(),
                Name = (kv.Value ?? "").Trim()
            })
            .Where(x => IsoA2.IsMatch(x.Code) && !string.IsNullOrWhiteSpace(x.Name))
            .ToDictionary(x => x.Code, x => x.Name, StringComparer.OrdinalIgnoreCase);

        if (map.Count == 0)
            throw new InvalidOperationException("No ISO A2 territories found in allcountryUK.json.");
        var countries = await db.Countries.ToListAsync(ct);

        var updated = 0;
        foreach (var c in countries)
        {
            if (!map.TryGetValue(c.Code, out var ukName))
                continue;

            if (string.Equals(c.Name, ukName, StringComparison.Ordinal))
                continue;

            c.Name = ukName;
            updated++;
        }

        if (updated == 0)
            return;

        await db.SaveChangesAsync(ct);
    }
}
