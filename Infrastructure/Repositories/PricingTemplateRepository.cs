using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PricingTemplateRepository : IPricingTemplateRepository
{
    private readonly CinemaDbContext _db;
    public PricingTemplateRepository(CinemaDbContext db) => _db = db;

    public async Task<List<PricingTemplate>> GetAllByHallAsync(int hallId, CancellationToken ct)
    {
        return await _db.PricingTemplates
            .AsNoTracking()
            .Where(x => x.HallId == hallId) 
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<List<PricingTemplate>> GetActiveByHallAsync(int hallId, CancellationToken ct)
    {
        return await _db.PricingTemplates
            .AsNoTracking()
            .Where(x => x.HallId == hallId && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
    }

    public async Task<PricingTemplate?> GetByIdWithDetailsAsync(int id, CancellationToken ct)
    {
        return await _db.PricingTemplates
            .AsNoTracking()
            .Include(x => x.RowPrices)
            .Include(x => x.CategoryMultipliers)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<PricingTemplate?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.PricingTemplates
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(PricingTemplate template, CancellationToken ct = default)
    {
        await _db.PricingTemplates.AddAsync(template, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PricingTemplate template, CancellationToken ct = default)
    {
        _db.PricingTemplates.Update(template);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(PricingTemplate template, CancellationToken ct = default)
    {
        _db.PricingTemplates.Remove(template);
        await _db.SaveChangesAsync(ct);
    }
}