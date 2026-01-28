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
        Task<List<Session>> GetAllAsync(DateTime? from, DateTime? to, int? hallId, int? movieId, bool includeCancelled, CancellationToken ct);

        Task AddAsync (Session session, CancellationToken ct);
        Task UpdateAsync(Session session, CancellationToken ct);
        Task DeleteAsync(Session session, CancellationToken ct);

        Task<bool> HasOverlapAsync(int hallId, DateTime start, DateTime end, int? ignoreSessionId, CancellationToken ct);
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
