namespace Web.ViewModels.Movies.Details
{
    public sealed class MovieScheduleDayVm
    {
        public DateOnly Date { get; init; }
        public IReadOnlyList<MovieSessionVm> Sessions { get; init; } = Array.Empty<MovieSessionVm>();
    }
}
