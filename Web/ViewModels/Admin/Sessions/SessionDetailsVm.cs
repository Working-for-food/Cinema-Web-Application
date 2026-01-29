using Infrastructure.Entities;

namespace Web.ViewModels.Admin.Sessions;

public class SessionDetailsVm
{
    public int Id { get; set; }
    public string MovieTitle { get; set; } = "";
    public string CinemaName { get; set; } = "";
    public string HallName { get; set; } = "";

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public PresentationType PresentationType { get; set; }
    public bool IsCancelled { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
