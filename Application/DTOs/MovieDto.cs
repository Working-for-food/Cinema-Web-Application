using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class MovieDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateOnly? ReleaseDate { get; set; }
        public int? Duration { get; set; }
        public string GenreNames { get; set; } = string.Empty;
        public int SessionsCount { get; set; }
    }

    public class MovieFormDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public int? Duration { get; set; }
        public string? ProductionCountryCode { get; set; }

        public List<int> GenreIds { get; set; } = new();
    }
}
