using Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces;

public interface IMovieImportRepository
{
    Task<Movie?> GetMovieByTmdbIdAsync(int tmdbId, CancellationToken ct);
    Task<Movie?> GetMovieByIdAsync(int id, CancellationToken ct);
    Task AddMovieAsync(Movie movie, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);

    // joins replace
    Task ReplaceMovieGenresAsync(int movieId, IReadOnlyList<int> genreIds, CancellationToken ct);
    Task ReplaceMovieCountriesAsync(int movieId, IReadOnlyList<string> countryCodes, CancellationToken ct);
    Task ReplaceMovieActorsAsync(int movieId, IReadOnlyList<(Person person, short order, string? character)> actors, CancellationToken ct);
    Task ReplaceMovieDirectorsAsync(int movieId, IReadOnlyList<(Person person, short order)> directors, CancellationToken ct);

    // upserts
    Task<Genre> UpsertGenreByTmdbAsync(int tmdbGenreId, string name, CancellationToken ct);
    Task<Person> UpsertPersonByTmdbAsync(int tmdbPersonId, string fullName, string? photoPath, CancellationToken ct);

    // lookup
    Task<IReadOnlyList<string>> FilterExistingCountryCodesAsync(IReadOnlyList<string> codes, CancellationToken ct);
}