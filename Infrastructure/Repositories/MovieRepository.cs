using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Interfaces;
using Infrastructure.Data; 
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly CinemaDbContext _context;

        public MovieRepository(CinemaDbContext context)
        {
            _context = context;
        }

        // GetAll
        public async Task<(IEnumerable<Movie> Items, int TotalCount)> GetAllAsync(
            string? searchTerm, string? sortBy, int page, int pageSize)
        {
            var query = _context.Movies
                .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(m => m.Title.Contains(searchTerm));
            }

            // Switch expression
            query = sortBy switch
            {
                "date_asc" => query.OrderBy(m => m.ReleaseDate),
                "date_desc" => query.OrderByDescending(m => m.ReleaseDate),
                "title_desc" => query.OrderByDescending(m => m.Title),
                _ => query.OrderBy(m => m.Title) // Default
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // GetById
        public async Task<Movie?> GetByIdAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.MovieGenres)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        // Add
        public async Task AddAsync(Movie movie, IEnumerable<int> genreIds)
        {
            
            foreach (var gId in genreIds)
            {
                movie.MovieGenres.Add(new MovieGenre { GenreId = gId });
            }

            await _context.Movies.AddAsync(movie);
            await _context.SaveChangesAsync();
        }

        // Update
        public async Task UpdateAsync(Movie movie, IEnumerable<int> genreIds)
        {
            var existingMovie = await _context.Movies
                .Include(m => m.MovieGenres)
                .FirstOrDefaultAsync(m => m.Id == movie.Id);

            if (existingMovie == null) return;

            
            existingMovie.Title = movie.Title;
            existingMovie.Description = movie.Description;
            existingMovie.ReleaseDate = movie.ReleaseDate;
            existingMovie.Duration = movie.Duration;
            existingMovie.ProductionCountryCode = movie.ProductionCountryCode;

            
            existingMovie.MovieGenres.Clear();

            
            foreach (var gId in genreIds)
            {
                existingMovie.MovieGenres.Add(new MovieGenre { MovieId = existingMovie.Id, GenreId = gId });
            }

            await _context.SaveChangesAsync();
        }

        // Delete
        public async Task DeleteAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }
        }

        // Genres

        // GetAllGenres
        public async Task<IEnumerable<Genre>> GetAllGenresAsync()
        {
            return await _context.Genres.OrderBy(g => g.Name).ToListAsync();
        }
    }
}
