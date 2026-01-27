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

        Task<List<SessionListDto>> GetAllAsync(
            DateTime? from,
            DateTime? to,
            int? hallId,
            int? movieId,
            bool includeCancelled,
            CancellationToken ct);

        Task<int> CreateAsync(SessionEditDto dto, CancellationToken ct);

        Task<bool> UpdateAsync(int id, SessionEditDto dto, CancellationToken ct);

        Task<bool> CancelAsync(int id, CancellationToken ct);

        Task<bool> RestoreAsync(int id, CancellationToken ct);
    }
}
