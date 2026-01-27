using Infrastructure.Entities;

namespace Application.DTOs
{
    public record SessionListDto
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = "";

        public int HallId {  get; set; }
        public string CinemaName { get; set; } = "";
        public string HallName { get; set; } = "";


        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public PresentationType PresentationType { get; set; }
        public bool IsCancelled { get; set; }
    }

}
