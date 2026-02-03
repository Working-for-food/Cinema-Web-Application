using Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record SessionEditDto
    {
        public int MovieId { get; init; }
        public int HallId { get; init; }
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public PresentationType PresentationType { get; init; }
    }
}
