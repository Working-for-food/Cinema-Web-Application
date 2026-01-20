using Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record SessionListDto(int Id, int MovieId, int HallId, DateTime StartTime, DateTime EndTime, PresentationType PresentationType, bool IsCancelled);

    public record SessionEditDto
    {
        public int MovieId { get; init; }
        public int HallId { get; init; }
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public PresentationType PresentationType { get; init; }
    }

}
