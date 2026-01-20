using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Infrastructure.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly CinemaDbContext _context;

        public SessionRepository (CinemaDbContext context)
        {
            _context = context;
        }

        public Task<Session?> GetByIdAsync (int id, CancellationToken ct)
        {
            return _context.Sessions.FirstOrDefaultAsync( x => x.Id == id, ct);
        }

        public async Task<List<Session>> GetAllAsync(DateTime? from, DateTime? to, int? hallId, int? movieId, bool includeCancelled, CancellationToken ct)
        {
            var query = _context.Sessions.AsQueryable();

            if (!includeCancelled) query.Where(x => !x.IsCancelled);
            if (from.HasValue) query.Where(x => x.StartTime >= from.Value);
            if (to.HasValue) query.Where(x => x.EndTime < to.Value);
            if (hallId.HasValue) query.Where(x => x.HallId == hallId.Value);
            if (movieId.HasValue) query.Where(x => x.MovieId == movieId.Value);

            return await query.OrderBy(x => x.StartTime).ToListAsync(ct);
        }

        public Task AddAsync (Session session, CancellationToken ct)
        {
            return _context.Sessions.AddAsync(session, ct).AsTask();
        }

        public void Update(Session session)
        {
            _context.Sessions.Update(session);
        }

        public void Delete(Session session)
        {
            _context.Sessions.Remove(session);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct)
        {
            return _context.SaveChangesAsync(ct);
        }

        public Task<bool> HasOverlapAsync(int hallId, DateTime start, DateTime end, int? ignoreSessionId, CancellationToken ct)
        {
            var query = _context.Sessions.Where(x => x.HallId == hallId && !x.IsCancelled);
            if (ignoreSessionId.HasValue) query.Where(x => x.Id != ignoreSessionId.Value);

            return query.AnyAsync(x => x.StartTime < end && x.EndTime > start, ct);
        }
    }
}
