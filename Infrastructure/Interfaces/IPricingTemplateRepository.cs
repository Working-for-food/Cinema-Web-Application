using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface IPricingTemplateRepository
{
    Task<List<PricingTemplate>> GetAllByHallAsync(int hallId, CancellationToken ct = default);
    Task<List<PricingTemplate>> GetActiveByHallAsync(int hallId, CancellationToken ct = default);
    Task<PricingTemplate?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<PricingTemplate?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(PricingTemplate template, CancellationToken ct = default);
    Task UpdateAsync(PricingTemplate template, CancellationToken ct = default);
    Task DeleteAsync(PricingTemplate template, CancellationToken ct = default);
}