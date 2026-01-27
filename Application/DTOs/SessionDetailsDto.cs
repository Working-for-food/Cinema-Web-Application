using Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record SessionDetailsDto
    {
        public int Id {  get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = "";
        public int HallId { get; set; }
        public string HallTitle { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public PresentationType PresentationType { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
