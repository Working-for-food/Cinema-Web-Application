using Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs;

public sealed class CinemaScheduleDto
{
    public int CinemaId { get; init; }
    public string CinemaName { get; init; } = "";
    public DateOnly SelectedDate { get; init; }

    public IReadOnlyList<ScheduleDayDto> Days { get; init; } = Array.Empty<ScheduleDayDto>();
    public IReadOnlyList<MovieScheduleCardDto> Movies { get; init; } = Array.Empty<MovieScheduleCardDto>();

    public sealed class ScheduleDayDto
    {
        public DateOnly Date { get; init; }
        public string LabelTop { get; init; } = "";
        public string LabelBottom { get; init; } = "";
        public bool IsActive { get; init; }
    }

    public sealed class MovieScheduleCardDto
    {
        public int MovieId { get; init; }
        public string Title { get; init; } = "";
        public string? PosterUrl { get; init; }
        public string? AgeLabel { get; init; }
        public IReadOnlyList<SessionTimeDto> Times { get; init; } = Array.Empty<SessionTimeDto>();
    }

    public sealed class SessionTimeDto
    {
        public int SessionId { get; init; }
        public TimeOnly Time { get; init; }
        public PresentationType PresentationType { get; init; }
    }
}
