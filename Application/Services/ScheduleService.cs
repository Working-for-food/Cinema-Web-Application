using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services;

public sealed class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _repo;

    public ScheduleService(IScheduleRepository repo) => _repo = repo;

    public async Task<CinemaScheduleDto> GetCinemaScheduleAsync(int cinemaId, DateOnly? selectedDate, CancellationToken ct)
    {
        var cinema = await _repo.GetCinemaAsync(cinemaId, ct);
        if (cinema is null)
            throw new InvalidOperationException("Cinema not found.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var selected = selectedDate ?? today;
        if (selected < today) selected = today;
        if (selected > today.AddDays(6)) selected = today.AddDays(6);

        var windowStart = today;
        var windowEndExclusive = today.AddDays(7);

        var from = windowStart.ToDateTime(TimeOnly.MinValue);
        var to = windowEndExclusive.ToDateTime(TimeOnly.MinValue);

        var now = DateTime.Now;
        if (from < now) from = now;
        var sessions = await _repo.GetUpcomingByCinemaAsync(cinemaId, from, to, ct);

        var culture = CultureInfo.GetCultureInfo("uk-UA");

        var days = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var d = windowStart.AddDays(i);
                return new CinemaScheduleDto.ScheduleDayDto
                {
                    Date = d,
                    LabelTop = d.ToString("dd MMMM", culture),
                    LabelBottom = BottomLabel(today, d, culture),
                    IsActive = d == selected
                };
            })
            .ToList();

        var dayFrom = selected.ToDateTime(TimeOnly.MinValue);
        var dayTo = selected.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var daySessions = sessions
            .Where(s => s.StartTime >= dayFrom && s.StartTime < dayTo);

        var movies = daySessions
            .GroupBy(s => s.MovieId)
            .Select(g =>
            {
                var first = g.First();
                var m = first.Movie;

                return new CinemaScheduleDto.MovieScheduleCardDto
                {
                    MovieId = m.Id,
                    Title = m.Title,
                    PosterUrl = PosterUrl(m.PosterPath),
                    AgeLabel = m.AgeRating.HasValue ? $"{m.AgeRating.Value}+" : null,
                    Times = g
                        .OrderBy(x => x.StartTime)
                        .Select(x => new CinemaScheduleDto.SessionTimeDto
                        {
                            SessionId = x.Id,
                            Time = TimeOnly.FromDateTime(x.StartTime),
                            PresentationType = x.PresentationType
                        })
                        .ToList()
                };
            })
            .OrderBy(x => x.Title)
            .ToList();

        return new CinemaScheduleDto
        {
            CinemaId = cinema.Id,
            CinemaName = cinema.Name,
            SelectedDate = selected,
            Days = days,
            Movies = movies
        };
    }

    private static string BottomLabel(DateOnly today, DateOnly d, CultureInfo culture)
    {
        if (d == today) return "сьогодні";
        if (d == today.AddDays(1)) return "завтра";
        return d.ToDateTime(TimeOnly.MinValue).ToString("dddd", culture);
    }

    private static string? PosterUrl(string? posterPath)
    {
        if (string.IsNullOrWhiteSpace(posterPath)) return null;
        if (posterPath.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return posterPath;

        var p = posterPath.StartsWith("/") ? posterPath : "/" + posterPath;
        return $"https://image.tmdb.org/t/p/w342{p}";
    }
}