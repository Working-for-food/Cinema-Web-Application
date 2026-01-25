using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly CinemaDbContext _context;

        public SessionRepository(CinemaDbContext context)
        {
            _context = context;
        }

        public Task<Session?> GetByIdAsync(int id, CancellationToken ct)
        {
            return _context.Sessions.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<List<Session>> GetAllAsync(
            DateTime? from,
            DateTime? to,
            int? hallId,
            int? movieId,
            bool includeCancelled,
            CancellationToken ct)
        {
            IQueryable<Session> query = _context.Sessions.AsNoTracking();

            if (!includeCancelled)
                query = query.Where(x => !x.IsCancelled);

            if (from.HasValue)
                query = query.Where(x => x.StartTime >= from.Value);

            if (to.HasValue)
                query = query.Where(x => x.EndTime <= to.Value);

            if (hallId.HasValue)
                query = query.Where(x => x.HallId == hallId.Value);

            if (movieId.HasValue)
                query = query.Where(x => x.MovieId == movieId.Value);

            return await query.OrderBy(x => x.StartTime).ToListAsync(ct);
        }

        public Task AddAsync(Session session, CancellationToken ct) => _context.Sessions.AddAsync(session, ct).AsTask();

        public void Update(Session session) => _context.Sessions.Update(session);

        public void Delete(Session session) => _context.Sessions.Remove(session);

        public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);

        public Task<bool> HasOverlapAsync(int hallId, DateTime start, DateTime end, int? ignoreSessionId, CancellationToken ct)
        {
            IQueryable<Session> query = _context.Sessions
                .AsNoTracking()
                .Where(x => x.HallId == hallId && !x.IsCancelled);

            if (ignoreSessionId.HasValue)
                query = query.Where(x => x.Id != ignoreSessionId.Value);

            return query.AnyAsync(x => x.StartTime < end && x.EndTime > start, ct);
        }
    }
}
