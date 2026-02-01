using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISessionService
    {
        Task<SessionDetailsDto?> GetByIdAsync(int id, CancellationToken ct);

        Task<PagedResult<SessionListDto>> GetAllPagedAsync(
            DateTime? from,
            DateTime? to,
            int? cinemaId,
            int? hallId,
            int? movieId,
            bool includeCancelled,
            bool includeFinished,
            string? sort,
            int page,
            CancellationToken ct);

        Task<int> CreateAsync(SessionEditDto dto, CancellationToken ct);

        Task<bool> UpdateAsync(int id, SessionEditDto dto, CancellationToken ct);

        Task<bool> CancelAsync(int id, CancellationToken ct);

        Task<bool> RestoreAsync(int id, CancellationToken ct);
    }
}
