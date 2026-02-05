namespace Application.Interfaces;

public interface IMetacriticService
{
    Task<string?> GetMetacriticAsync(string imdbId, CancellationToken ct = default);
}
