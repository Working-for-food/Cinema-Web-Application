using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Infrastructure.Data.Seed;

public static class CountrySeeder
{
    private sealed class SourceCountry
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("alpha-2")]
        public string? Alpha2 { get; set; }
    }
    public static async Task SeedAsync(CinemaDbContext db, CancellationToken ct = default)
    {
        if (await db.Countries.AsNoTracking().AnyAsync(ct))
            return;

        var path = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "allcountry.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("allcountry.json not found", path);

        await using var stream = File.OpenRead(path);

        var source = await JsonSerializer.DeserializeAsync<List<SourceCountry>>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            ct
        ) ?? throw new InvalidOperationException("allcountry.json is empty or invalid JSON.");

        var mapped = source
            .Select(x => new
            {
                Code = (x.Alpha2 ?? "").Trim().ToUpperInvariant(),
                Name = (x.Name ?? "").Trim()
            })
            .Where(x => x.Code.Length == 2 && !string.IsNullOrWhiteSpace(x.Name))
            .ToList();

        var dupCodes = mapped.GroupBy(x => x.Code).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (dupCodes.Count > 0)
            throw new InvalidOperationException($"Duplicate country codes in seed: {string.Join(", ", dupCodes)}");

        var entities = mapped.Select(x => new Country { Code = x.Code, Name = x.Name }).ToList();

        await db.Countries.AddRangeAsync(entities, ct);
        await db.SaveChangesAsync(ct);
    }
}
