using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Entities;

namespace Infrastructure.Interfaces
{
    public interface IMovieRepository
    {
        Task<(IEnumerable<Movie> Items, int TotalCount)> GetAllAsync(
            string? searchTerm,
            string? sortBy,
            int page,
            int pageSize,
            CancellationToken ct = default);

        Task<Movie?> GetByIdAsync(int id, CancellationToken ct = default);

        // for details screen (includes genres + sessions)
        Task<Movie?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);

        Task AddAsync(Movie movie, IEnumerable<int> genreIds, CancellationToken ct = default);
        Task UpdateAsync(Movie movie, IEnumerable<int> genreIds, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        // "status" check: is this movie used in sessions?
        Task<bool> AnySessionsAsync(int movieId, CancellationToken ct = default);
    }
}
