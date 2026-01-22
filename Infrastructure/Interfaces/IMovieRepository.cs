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
            int pageSize);

        Task<Movie?> GetByIdAsync(int id);
        Task AddAsync(Movie movie, IEnumerable<int> genreIds);
        Task UpdateAsync(Movie movie, IEnumerable<int> genreIds);
        Task DeleteAsync(int id);

        // Genres
        Task<IEnumerable<Genre>> GetAllGenresAsync();
    }
}
