using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Infrastructure.Interfaces;
using Infrastructure.Entities;

namespace Application.Services
{
    public class MovieService
    {
        private readonly IMovieRepository _repository;

        public MovieService(IMovieRepository repository)
        {
            _repository = repository;
        }

        // GetAll
        public async Task<(IEnumerable<MovieDto> List, int TotalCount)> GetMoviesAsync(
            string? search, string sortBy, int page)
        {
            var result = await _repository.GetAllAsync(search, sortBy, page, 10); // PageSize = 10

            var dtos = result.Items.Select(m => new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                ReleaseDate = m.ReleaseDate,
                Duration = m.Duration,
                GenreNames = string.Join(", ", m.MovieGenres.Select(mg => mg.Genre.Name))
            });

            return (dtos, result.TotalCount);
        }

        // GetById
        public async Task<MovieFormDto> GetMovieForEditAsync(int id)
        {
            var movie = await _repository.GetByIdAsync(id);
            if (movie == null) throw new Exception("Movie not found");

            return new MovieFormDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                Duration = movie.Duration,
                ReleaseDate = movie.ReleaseDate,
                ProductionCountryCode = movie.ProductionCountryCode,
                GenreIds = movie.MovieGenres.Select(mg => mg.GenreId).ToList()
            };
        }

        // Create + Validate
        public async Task CreateMovieAsync(MovieFormDto dto)
        {
            ValidateMovie(dto);

            var movie = new Movie
            {
                Title = dto.Title,
                Description = dto.Description,
                ReleaseDate = dto.ReleaseDate,
                Duration = dto.Duration,
                ProductionCountryCode = dto.ProductionCountryCode
            };

            await _repository.AddAsync(movie, dto.GenreIds);
        }

        // Update + Validate
        public async Task UpdateMovieAsync(MovieFormDto dto)
        {
            ValidateMovie(dto);

            var movie = new Movie
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                ReleaseDate = dto.ReleaseDate,
                Duration = dto.Duration,
                ProductionCountryCode = dto.ProductionCountryCode
            };

            await _repository.UpdateAsync(movie, dto.GenreIds);
        }

        // Delete
        public async Task DeleteMovieAsync(int id) => await _repository.DeleteAsync(id);


        public async Task<IEnumerable<Genre>> GetGenresAsync() => await _repository.GetAllGenresAsync();

        // Validate
        private void ValidateMovie(MovieFormDto dto)
        {
            if (dto.Duration.HasValue && dto.Duration <= 0)
                throw new Exception("Duration must be positive.");

            
        }
    }
}
