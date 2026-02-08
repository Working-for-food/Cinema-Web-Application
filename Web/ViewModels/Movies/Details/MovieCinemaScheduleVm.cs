namespace Web.ViewModels.Movies.Details
{
    public sealed class MovieCinemaScheduleVm
    {
        public int CinemaId { get; set; }
        public string CinemaName { get; set; } = "";
        public IReadOnlyList<MovieScheduleDayVm> Days { get; set; } = Array.Empty<MovieScheduleDayVm>();
    }
}
