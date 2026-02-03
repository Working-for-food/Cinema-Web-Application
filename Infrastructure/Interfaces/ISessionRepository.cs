using Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface ISessionRepository
    {
        Task<Session?> GetByIdAsync(int id, CancellationToken ct);

        Task<(IReadOnlyList<Session> Items, int TotalCount)> GetAllPagedAsync(
            DateTime? from,
            DateTime? toExclusive,
            int? cinemaId,
            int? hallId,
            int? movieId,
            bool includeCancelled,
            bool includeFinished,
            DateTime asOf,
            string? sort,
            int page,
            int pageSize,
            CancellationToken ct);

        Task AddAsync (Session session, CancellationToken ct);
        Task UpdateAsync(Session session, CancellationToken ct);

        Task<bool> HasOverlapAsync(int hallId, DateTime start, DateTime end, int? ignoreSessionId, CancellationToken ct);
    }
}
