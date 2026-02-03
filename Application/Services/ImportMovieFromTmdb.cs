using Application.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Interfaces;
namespace Application.Services;

public sealed class ImportMovieFromTmdb : IImportMovieFromTmdb
{
    private readonly ITmdbClient _tmdb;
    private readonly IMovieImportRepository _repo;

    public ImportMovieFromTmdb(ITmdbClient tmdb, IMovieImportRepository repo)
    {
        _tmdb = tmdb;
        _repo = repo;
    }

    public async Task<int> ImportAsync(int tmdbId, CancellationToken ct = default)
    {
        var details = await _tmdb.GetMovieDetailsAsync(tmdbId, ct);
        var credits = await _tmdb.GetCreditsAsync(tmdbId, ct);
        var videos = await _tmdb.GetVideosAsync(tmdbId, ct);
        var releaseDates = await _tmdb.GetReleaseDatesAsync(tmdbId, ct);

        var trailer = videos.Results.FirstOrDefault(v =>
            v.Site == "YouTube" && v.Type == "Trailer" && !string.IsNullOrWhiteSpace(v.Key));
        var trailerUrl = trailer is null ? null : $"https://www.youtube.com/watch?v={trailer.Key}";

        DateOnly? releaseDate = null;
        if (!string.IsNullOrWhiteSpace(details.ReleaseDate) &&
            DateOnly.TryParse(details.ReleaseDate, out var parsed))
            releaseDate = parsed;

        var movie = await _repo.GetMovieByTmdbIdAsync(tmdbId, ct);

        if (movie is null)
        {
            movie = new Infrastructure.Entities.Movie { TmdbId = details.Id };
            await _repo.AddMovieAsync(movie, ct);
        }
        else if (movie.IsDeleted)
        {
            movie.IsDeleted = false; // restore
        }

        movie.Title = details.Title ?? "(no title)";
        movie.OriginalName = details.OriginalTitle;
        movie.Description = details.Overview;
        movie.ReleaseDate = releaseDate;
        movie.Duration = details.Runtime;
        movie.PosterPath = details.PosterPath;
        movie.BackdropPath = details.BackdropPath;
        movie.Rating = details.VoteAverage;
        movie.TrailerUrl = trailerUrl;
        movie.AgeRating = TmdbAgeRatingHelper.GetAgeRating(releaseDates);

        await _repo.SaveChangesAsync(ct); // щоб отримати movie.Id

        // Genres
        if (details.Genres.Count > 0)
        {
            var genreIds = new List<int>(details.Genres.Count);
            foreach (var g in details.Genres)
            {
                var name = string.IsNullOrWhiteSpace(g.Name) ? "—" : g.Name!;
                var genre = await _repo.UpsertGenreByTmdbAsync(g.Id, name, ct);
                genreIds.Add(genre.Id);
            }

            await _repo.ReplaceMovieGenresAsync(movie.Id, genreIds, ct);
        }

        // Countries (тільки ті що є в seeded Countries)
        if (details.ProductionCountries.Count > 0)
        {
            var codes = details.ProductionCountries
                .Select(c => (c.Iso3166_1 ?? "").Trim().ToUpperInvariant())
                .Where(code => code.Length == 2)
                .Distinct()
                .ToList();

            var existingCodes = await _repo.FilterExistingCountryCodesAsync(codes, ct);
            await _repo.ReplaceMovieCountriesAsync(movie.Id, existingCodes, ct);
        }

        // Actors (Top 7)
        if (credits.Cast.Count > 0)
        {
            var actors = new List<(Infrastructure.Entities.Person person, short order, string? character)>();
            foreach (var c in credits.Cast.OrderBy(x => x.Order).Take(7))
            {
                var fullName = string.IsNullOrWhiteSpace(c.Name) ? "Unknown" : c.Name!.Trim();
                var person = await _repo.UpsertPersonByTmdbAsync(c.Id, fullName, c.ProfilePath, ct);

                actors.Add((
                    person,
                    (short)Math.Clamp(c.Order, 0, short.MaxValue),
                    c.Character
                ));
            }

            await _repo.ReplaceMovieActorsAsync(movie.Id, actors, ct);
        }

        // Directors
        var directorsCrew = credits.Crew
            .Where(x => string.Equals(x.Job, "Director", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (directorsCrew.Count > 0)
        {
            var directors = new List<(Infrastructure.Entities.Person person, short order)>();
            for (var i = 0; i < directorsCrew.Count; i++)
            {
                var d = directorsCrew[i];
                var fullName = string.IsNullOrWhiteSpace(d.Name) ? "Unknown" : d.Name!.Trim();
                var person = await _repo.UpsertPersonByTmdbAsync(d.Id, fullName, d.ProfilePath, ct);

                directors.Add((person, (short)Math.Clamp(i, 0, short.MaxValue)));
            }

            await _repo.ReplaceMovieDirectorsAsync(movie.Id, directors, ct);
        }
        else
        {
            await _repo.ReplaceMovieDirectorsAsync(movie.Id, Array.Empty<(Infrastructure.Entities.Person, short)>(), ct);
        }

        await _repo.SaveChangesAsync(ct);
        return movie.Id;
    }
}
