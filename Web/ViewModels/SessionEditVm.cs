using Infrastructure.Entities;

namespace Web.ViewModels
{
    public class SessionEditVm
    {
        //Add data anotation!

        public int MovieId { get; set; }
        public int HallId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public PresentationType PresentationType { get; set; }
    }
}
