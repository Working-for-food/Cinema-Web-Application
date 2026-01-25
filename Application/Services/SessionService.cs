using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Application.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _repo;

    public SessionService(ISessionRepository repo)
    {
        _repo = repo;
    }

    public async Task<SessionDetailsDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var s = await _repo.GetByIdAsync(id, ct);
        if (s is null) return null;

        return new SessionDetailsDto
        {
            Id = s.Id,
            MovieId = s.MovieId,
            HallId = s.HallId,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            PresentationType = s.PresentationType,
            IsCancelled = s.IsCancelled,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
    }

    public async Task<List<SessionListDto>> GetAllAsync(
        DateTime? from,
        DateTime? to,
        int? hallId,
        int? movieId,
        bool includeCancelled,
        CancellationToken ct)
    {
        var list = await _repo.GetAllAsync(from, to, hallId, movieId, includeCancelled, ct);

        return list.Select(s => new SessionListDto
        {
            Id = s.Id,
            MovieId = s.MovieId,
            HallId = s.HallId,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            PresentationType = s.PresentationType,
            IsCancelled = s.IsCancelled
        }).ToList();
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
        await _repo.SaveChangesAsync(ct);

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(int id, SessionEditDto dto, CancellationToken ct)
    {
        ValidateTimeRange(dto.StartTime, dto.EndTime);

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return false;

        if (entity.IsCancelled)
            throw new InvalidOperationException("Не можна редагувати скасований сеанс. Спочатку відновіть його.");

        var hasOverlap = await _repo.HasOverlapAsync(dto.HallId, dto.StartTime, dto.EndTime, ignoreSessionId: id, ct);
        if (hasOverlap)
            throw new InvalidOperationException("У цьому залі вже є сеанс, що перетинається за часом.");

        entity.MovieId = dto.MovieId;
        entity.HallId = dto.HallId;
        entity.StartTime = dto.StartTime;
        entity.EndTime = dto.EndTime;
        entity.PresentationType = dto.PresentationType;
        entity.UpdatedAt = DateTime.UtcNow;

        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> CancelAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return false;

        if (!entity.IsCancelled)
        {
            entity.IsCancelled = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _repo.Update(entity);
            await _repo.SaveChangesAsync(ct);
        }

        return true;
    }

    public async Task<bool> RestoreAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return false;

        if (entity.IsCancelled)
        {
            var hasOverlap = await _repo.HasOverlapAsync(entity.HallId, entity.StartTime, entity.EndTime, ignoreSessionId: id, ct);
            if (hasOverlap)
                throw new InvalidOperationException("Неможливо відновити: у цьому залі вже є інший сеанс, що перетинається.");

            entity.IsCancelled = false;
            entity.UpdatedAt = DateTime.UtcNow;

            _repo.Update(entity);
            await _repo.SaveChangesAsync(ct);
        }

        return true;
    }

    private static void ValidateTimeRange(DateTime start, DateTime end)
    {
        if (start >= end)
            throw new ArgumentException("StartTime має бути раніше EndTime.");
    }
}
