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
            return _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall).ThenInclude(h => h.Cinema)
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<(List<Session> Items, int TotalCount)> GetAllPagedAsync(
            DateTime? from,
            DateTime? toExclusive,
            int? hallId,
            int? movieId,
            bool includeCancelled,
            string? sort,
            int page,
            int pageSize,
            CancellationToken ct)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            IQueryable<Session> query = _context.Sessions
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Cinema);

            if (!includeCancelled)
                query = query.Where(x => !x.IsCancelled);

            if (from.HasValue)
                query = query.Where(x => x.StartTime >= from.Value);

            if (toExclusive.HasValue)
                query = query.Where(x => x.StartTime < toExclusive.Value);

            if (hallId.HasValue)
                query = query.Where(x => x.HallId == hallId.Value);

            if (movieId.HasValue)
                query = query.Where(x => x.MovieId == movieId.Value);

            var totalCount = await query.CountAsync(ct);

            query = ApplySort(query, sort);

            var skip = (page - 1) * pageSize;

            var items = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        private static IQueryable<Session> ApplySort(IQueryable<Session> query, string? sort)
        {
            return (sort ?? "start_asc").ToLowerInvariant() switch
            {
                "start_desc" => query.OrderByDescending(x => x.StartTime).ThenByDescending(x => x.EndTime).ThenByDescending(x => x.Id),
                "end_asc" => query.OrderBy(x => x.EndTime).ThenBy(x => x.StartTime).ThenBy(x => x.Id),
                "end_desc" => query.OrderByDescending(x => x.EndTime).ThenByDescending(x => x.StartTime).ThenByDescending(x => x.Id),
                _ => query.OrderBy(x => x.StartTime).ThenBy(x => x.EndTime).ThenBy(x => x.Id),
            };
        }

        public async Task AddAsync(Session session, CancellationToken ct)
        {
            await _context.Sessions.AddAsync(session, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Session session, CancellationToken ct)
        {
            _context.Sessions.Update(session);
            await _context.SaveChangesAsync(ct);
        }

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
