using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Application.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _repo;
    private const int PageSize = 20;

    public SessionService(ISessionRepository repo) => _repo = repo;

    public async Task<SessionDetailsDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var s = await _repo.GetByIdAsync(id, ct);
        if (s is null) return null;

        return new SessionDetailsDto
        {
            Id = s.Id,
            MovieId = s.MovieId,
            MovieTitle = s.Movie?.Title ?? "",

            HallName = s.Hall?.Name ?? "",
            HallId = s.HallId,

            CinemaName = s.Hall?.Cinema?.Name ?? "",
            CinemaId = s.Hall?.CinemaId ?? 0,

            StartTime = s.StartTime,
            EndTime = s.EndTime,
            PresentationType = s.PresentationType,
            IsCancelled = s.IsCancelled,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
    }

    public async Task<SessionEditDto?> GetForEditAsync(int id, CancellationToken ct)
    {
        var s = await _repo.GetByIdAsync(id, ct);
        if (s is null) return null;

        return new SessionEditDto
        {
            MovieId = s.MovieId,
            HallId = s.HallId,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            PresentationType = s.PresentationType
        };
    }

    public async Task<PagedResult<SessionListDto>> GetAllPagedAsync(
    DateTime? from,
    DateTime? to,
    int? cinemaId,
    int? hallId,
    int? movieId,
    bool includeCancelled,
    bool includeFinished,
    string? sort,
    int page,
    CancellationToken ct)
    {
        if (page < 1) page = 1;

        var fromNorm = from?.Date;
        var toExclusive = to?.Date.AddDays(1);

        var asOf = DateTime.Now;

        var (items, totalCount) = await _repo.GetAllPagedAsync(
            fromNorm,
            toExclusive,
            cinemaId,
            hallId,
            movieId,
            includeCancelled,
            includeFinished,
            asOf,
            sort,
            page,
            PageSize,
            ct);

        var dtos = items.Select(s => new SessionListDto
        {
            Id = s.Id,
            MovieId = s.MovieId,
            HallId = s.HallId,

            MovieTitle = s.Movie?.Title,
            PosterPath = s.Movie?.PosterPath,
            HallName = s.Hall?.Name,
            CinemaName = s.Hall?.Cinema?.Name,

            StartTime = s.StartTime,
            EndTime = s.EndTime,
            PresentationType = s.PresentationType,
            IsCancelled = s.IsCancelled
        }).ToList();

        return new PagedResult<SessionListDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = PageSize
        };
    }

    public async Task<int> CreateAsync(SessionEditDto dto, CancellationToken ct)
    {
        ValidateTimeRange(dto.StartTime, dto.EndTime);

        var hasOverlap = await _repo.HasOverlapAsync(dto.HallId, dto.StartTime, dto.EndTime, ignoreSessionId: null, ct);
        if (hasOverlap)
            throw new InvalidOperationException("У цьому залі вже є сеанс, що перетинається за часом.");

        var entity = new Session
        {
            MovieId = dto.MovieId,
            HallId = dto.HallId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            PresentationType = dto.PresentationType,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        await _repo.AddAsync(entity, ct);

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(int id, SessionEditDto dto, CancellationToken ct)
    {
        ValidateTimeRange(dto.StartTime, dto.EndTime);

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return false;

        if (entity.IsCancelled)
            throw new InvalidOperationException("Не можна редагувати скасований сеанс. Спочатку відновіть його.");

        EnsureNotOver(entity, "редагувати сеанс");

        var hasOverlap = await _repo.HasOverlapAsync(dto.HallId, dto.StartTime, dto.EndTime, ignoreSessionId: id, ct);
        if (hasOverlap)
            throw new InvalidOperationException("У цьому залі вже є сеанс, що перетинається за часом.");

        entity.MovieId = dto.MovieId;
        entity.HallId = dto.HallId;
        entity.StartTime = dto.StartTime;
        entity.EndTime = dto.EndTime;
        entity.PresentationType = dto.PresentationType;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(entity, ct);

        return true;
    }

    public async Task<bool> CancelAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return false;

        EnsureNotOver(entity, "скасувати сеанс");

        if (!entity.IsCancelled)
        {
            entity.IsCancelled = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(entity, ct);
        }

        return true;
    }

    public async Task<bool> RestoreAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return false;

        EnsureNotOver(entity, "відновити сеанс");

        if (entity.IsCancelled)
        {
            var hasOverlap = await _repo.HasOverlapAsync(entity.HallId, entity.StartTime, entity.EndTime, ignoreSessionId: id, ct);
            if (hasOverlap)
                throw new InvalidOperationException("Неможливо відновити сеанс: у цьому залі вже є інший сеанс, що перетинається за часом.");

            entity.IsCancelled = false;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(entity, ct);
        }

        return true;
    }

    private static void ValidateTimeRange(DateTime start, DateTime end)
    {
        if (start >= end)
            throw new ArgumentException("Час початку має бути раніше часу завершення.");
    }
    private static bool IsOver(DateTime endTime)
    {
        return endTime <= DateTime.Now;
    }

    private static void EnsureNotOver(Session s, string action)
    {
        if (IsOver(s.EndTime))
            throw new InvalidOperationException($"Не можна {action}: сеанс уже завершився.");
    }
}
