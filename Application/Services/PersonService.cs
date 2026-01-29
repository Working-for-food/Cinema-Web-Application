using Application.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Application.Services;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _people;
    private readonly ICountryRepository _countries;

    public PersonService(IPersonRepository people, ICountryRepository countries)
    {
        _people = people;
        _countries = countries;
    }

    public Task<(IReadOnlyList<Person> Items, int TotalCount)> GetAllAsync(string? search, int page, int pageSize, CancellationToken ct = default) =>
        _people.GetAllAsync(search, page, pageSize, ct);

    public Task<Person?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _people.GetByIdAsync(id, ct);

    public async Task<(bool ok, string? error)> CreateAsync(Person person, CancellationToken ct = default)
    {
        var err = await ValidateAsync(person, ct);
        if (err != null) return (false, err);

        await _people.AddAsync(person, ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> UpdateAsync(Person person, CancellationToken ct = default)
    {
        if (person.Id <= 0) return (false, "Invalid person id.");

        var existing = await _people.GetByIdAsync(person.Id, ct);
        if (existing == null) return (false, "Person not found.");

        var err = await ValidateAsync(person, ct);
        if (err != null) return (false, err);

        existing.FirstName = person.FirstName;
        existing.MiddleName = person.MiddleName;
        existing.LastName = person.LastName;
        existing.BirthDate = person.BirthDate;
        existing.CountryCode = person.CountryCode;
        existing.PhotoUrl = person.PhotoUrl;

        await _people.UpdateAsync(existing, ct);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        var person = await _people.GetByIdAsync(id, ct);
        if (person == null) return (false, "Person not found.");

        var used = await _people.IsUsedAsync(id, ct);
        if (used) return (false, "Cannot delete: person is referenced by movies.");

        await _people.DeleteAsync(person, ct);
        return (true, null);
    }

    private async Task<string?> ValidateAsync(Person p, CancellationToken ct)
    {
        p.FirstName = (p.FirstName ?? "").Trim();
        p.MiddleName = string.IsNullOrWhiteSpace(p.MiddleName) ? null : p.MiddleName.Trim();
        p.LastName = (p.LastName ?? "").Trim();
        p.CountryCode = string.IsNullOrWhiteSpace(p.CountryCode) ? null : p.CountryCode.Trim().ToUpperInvariant();
        p.PhotoUrl = string.IsNullOrWhiteSpace(p.PhotoUrl) ? null : p.PhotoUrl.Trim();
        var middle = p.MiddleName is null ? "" : $" {p.MiddleName}";
        p.FullName = $"{p.FirstName}{middle} {p.LastName}".Trim();

        if (string.IsNullOrWhiteSpace(p.FirstName) || p.FirstName.Length > 60) return "First name is required (<= 60).";
        if (p.MiddleName != null && p.MiddleName.Length > 60) return "Middle name must be <= 60.";
        if (string.IsNullOrWhiteSpace(p.LastName) || p.LastName.Length > 60) return "Last name is required (<= 60).";
        if (p.PhotoUrl != null && p.PhotoUrl.Length > 700) return "PhotoUrl is too long (<= 700).";

        if (p.BirthDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (p.BirthDate.Value > today) return "BirthDate cannot be in the future.";
            if (p.BirthDate.Value < new DateOnly(1850, 1, 1)) return "BirthDate looks too old.";
        }

        if (p.CountryCode != null)
        {
            if (p.CountryCode.Length != 2 || !p.CountryCode.All(char.IsLetter))
                return "CountryCode must be 2 letters (e.g. UA).";

            var exists = await _countries.GetByCodeAsync(p.CountryCode, ct);
            if (exists == null) return "Selected country does not exist.";
        }

        return null;
    }
}
