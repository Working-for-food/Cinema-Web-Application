namespace Application.Interfaces;
public interface IImportMovieFromTmdb
{
    Task<int> ImportAsync(int tmdbId, CancellationToken ct = default);
}