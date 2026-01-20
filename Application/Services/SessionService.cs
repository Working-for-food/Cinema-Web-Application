using Application.DTOs;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services;

public class SessionService
{
    private readonly ISessionRepository _repo;
    public SessionService(ISessionRepository repo) => _repo = repo;

    public async Task<List<SessionListDto>> GetAllAsync(DateTime? from, DateTime? to, int? hallId, int? movieId, bool includeCancelled, CancellationToken ct)
    {
       var sessions = await _repo.GetAllAsync(from, to, hallId, movieId, includeCancelled, ct);

        var dtos = sessions.Select(x => new SessionListDto(
                x.Id,
                x.MovieId,
                x.HallId,
                x.StartTime,
                x.EndTime,
                x.PresentationType,
                x.IsCancelled
        ));

        return dtos.ToList();
    }

    public async Task<int> CreateAsync(SessionEditDto dto, CancellationToken ct)
    {
        ValidateTimes(dto.StartTime, dto.EndTime);

        if (await _repo.HasOverlapAsync(dto.HallId, dto.StartTime, dto.EndTime, null, ct))
        {
            throw new InvalidOperationException("Session overlaps with existing session in this hall.");
        }

        var entity = new Session
        {
            MovieId = dto.MovieId,
            HallId = dto.HallId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            PresentationType = dto.PresentationType,
            IsCancelled = false,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task UpdateAsync(int id, SessionEditDto dto, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Session not found.");

        ValidateTimes(dto.StartTime, dto.EndTime);

        if (await _repo.HasOverlapAsync(dto.HallId, dto.StartTime, dto.EndTime, id, ct))
            throw new InvalidOperationException("Session overlaps with existing session in this hall.");

        entity.MovieId = dto.MovieId;
        entity.HallId = dto.HallId;
        entity.StartTime = dto.StartTime;
        entity.EndTime = dto.EndTime;
        entity.PresentationType = dto.PresentationType;
        entity.UpdatedAt = DateTime.UtcNow;

        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Session not found.");

        entity.IsCancelled = true;
        entity.UpdatedAt = DateTime.UtcNow;

        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);
    }
    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Session not found.");

        _repo.Delete(entity);
        await _repo.SaveChangesAsync(ct);
    }
    private static void ValidateTimes(DateTime start, DateTime end)
    {
        if (start >= end)
        {
            throw new ArgumentException("StartTime must be earlier than EndTime.");
        }
    }
}
