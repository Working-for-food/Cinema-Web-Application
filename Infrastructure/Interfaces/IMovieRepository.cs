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

        Task<Movie?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
        Task<string?> GetTitleByIdAsync(int id, CancellationToken ct);

        Task AddAsync(Movie movie, IEnumerable<int> genreIds, IEnumerable<int> actorIds, IEnumerable<string> countryCodes, IEnumerable<int> directorIds, CancellationToken ct = default);
        Task UpdateAsync(Movie movie, IEnumerable<int> genreIds, IEnumerable<int> actorIds, IEnumerable<string> countryCodes, IEnumerable<int> directorIds, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        Task<bool> AnySessionsAsync(int movieId, CancellationToken ct = default);

        //search by title
        Task<List<Movie>> SearchAsync(string? query, int take, CancellationToken ct);
    }
}
