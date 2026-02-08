namespace Web.ViewModels.Movies.Details
{
    public sealed class MovieSessionVm
    {
        public int Id { get; init; }
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }
        public string HallName { get; init; } = "";
        public string Presentation { get; init; } = ""; // "2D"/"3D"/"IMAX"
    }
}
