using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
<<<<<<< HEAD
using System.Text.Json;
using System.Text.Json.Serialization;
=======
using System.Text.Json.Serialization;
using System.Text.Json;
>>>>>>> origin/A4-A5-countries-people-admin

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

        
        var existing = await db.Countries.AsNoTracking().ToListAsync(ct);
        var existingByCode = existing.ToDictionary(x => x.Code, x => x, StringComparer.OrdinalIgnoreCase);

        
        var toAdd = new List<Country>();
        var toUpdate = new List<Country>();

        foreach (var c in mapped)
        {
            if (!existingByCode.TryGetValue(c.Code, out var ex))
            {
                toAdd.Add(new Country { Code = c.Code, Name = c.Name });
                continue;
            }

            if (!string.Equals(ex.Name, c.Name, StringComparison.Ordinal))
            {
                toUpdate.Add(new Country { Code = ex.Code, Name = c.Name });
            }
        }

        if (toAdd.Count == 0 && toUpdate.Count == 0)
            return;

        if (toAdd.Count > 0)
            await db.Countries.AddRangeAsync(toAdd, ct);

        if (toUpdate.Count > 0)
            db.Countries.UpdateRange(toUpdate);

        await db.SaveChangesAsync(ct);
    }
}
