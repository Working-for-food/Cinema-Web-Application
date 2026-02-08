using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface IAfishaRepository
{
    Task<List<Movie>> GetNowShowingAsync(CancellationToken ct = default);
    Task<List<Movie>> GetComingSoonAsync(CancellationToken ct = default);
}
