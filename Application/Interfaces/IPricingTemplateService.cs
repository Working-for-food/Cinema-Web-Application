using Application.DTOs.Pricing;

namespace Application.Interfaces;

public interface IPricingTemplateService
{
    Task<List<PricingTemplateListItemDto>> GetListForHallAsync(int hallId, CancellationToken ct = default);
    Task<List<PricingTemplateListItemDto>> GetActiveListForHallAsync(int hallId, CancellationToken ct = default);
    Task<ApplyPricingTemplateResultDto?> GetTemplateDataAsync(int templateId, CancellationToken ct = default);
    Task ToggleStatusAsync(int id, CancellationToken ct = default);
    Task CreateAsync(PricingTemplateEditDto dto, CancellationToken ct = default);
}