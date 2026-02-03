using Infrastructure.Entities;

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
