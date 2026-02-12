using Application.DTOs.Pricing;
using Application.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Application.Services;

public class PricingTemplateService : IPricingTemplateService
{
    private readonly IPricingTemplateRepository _repo;

    public PricingTemplateService(IPricingTemplateRepository repo) => _repo = repo;

    private static List<PricingTemplateListItemDto> MapToListDto(List<PricingTemplate> list)
    {
        return list.Select(x => new PricingTemplateListItemDto
        {
            Id = x.Id,
            Name = x.Name,
            IsActive = x.IsActive,
            HallId = x.HallId
        }).ToList();
    }

    public async Task<List<PricingTemplateListItemDto>> GetListForHallAsync(int hallId, CancellationToken ct)
    {
        var list = await _repo.GetAllByHallAsync(hallId, ct);
        return MapToListDto(list);
    }

    public async Task<List<PricingTemplateListItemDto>> GetActiveListForHallAsync(int hallId, CancellationToken ct)
    {
        var list = await _repo.GetActiveByHallAsync(hallId, ct);
        return MapToListDto(list);
    }

    public async Task<PricingTemplateEditDto?> GetForEditAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdWithDetailsAsync(id, ct);
        if (entity == null) return null;

        return new PricingTemplateEditDto
        {
            Id = entity.Id,
            Name = entity.Name,
            IsActive = entity.IsActive,
            HallId = entity.HallId,
            RowPrices = entity.RowPrices
                .OrderBy(x => x.Row)
                .Select(x => new RowPriceDto { Row = x.Row, BasePrice = x.BasePrice })
                .ToList(),
            CategoryMultipliers = entity.CategoryMultipliers
                .Select(x => new CategoryMultiplierDto { Category = (int)x.Category, Multiplier = x.Multiplier })
                .ToList()
        };
    }

    public async Task<ApplyPricingTemplateResultDto?> GetTemplateDataAsync(int templateId, CancellationToken ct)
    {
        var entity = await _repo.GetByIdWithDetailsAsync(templateId, ct);
        if (entity is null) return null;

        return new ApplyPricingTemplateResultDto
        {
            PricingTemplateId = entity.Id,
            RowPrices = entity.RowPrices
                .Select(x => new RowPriceDto { Row = x.Row, BasePrice = x.BasePrice })
                .OrderBy(x => x.Row)
                .ToList(),
            CategoryMultipliers = entity.CategoryMultipliers
                .Select(x => new CategoryMultiplierDto { Category = (int)x.Category, Multiplier = x.Multiplier })
                .OrderBy(x => x.Category)
                .ToList()
        };
    }

    public async Task ToggleStatusAsync(int id, CancellationToken ct)
    {
        var template = await _repo.GetByIdAsync(id, ct);
        if (template == null) throw new Exception("Шаблон не знайдено");

        template.IsActive = !template.IsActive;

        await _repo.UpdateAsync(template, ct);
    }

    public async Task CreateAsync(PricingTemplateEditDto dto, CancellationToken ct)
    {
        var entity = new PricingTemplate
        {
            Name = dto.Name,
            HallId = dto.HallId,
            IsActive = dto.IsActive,

            RowPrices = dto.RowPrices.Select(r => new PricingTemplateRowPrice
            {
                Row = r.Row,
                BasePrice = r.BasePrice
            }).ToList(),

            CategoryMultipliers = dto.CategoryMultipliers.Select(c => new PricingTemplateCategoryMultiplier
            {
                Category = (SeatCategory)c.Category,
                Multiplier = c.Multiplier
            }).ToList()
        };

        await _repo.AddAsync(entity, ct);
    }

    public async Task UpdateAsync(PricingTemplateEditDto dto, CancellationToken ct)
    {
        var entity = await _repo.GetByIdWithDetailsAsync(dto.Id, ct);
        if (entity == null) throw new Exception("Шаблон не знайдено");

        entity.Name = dto.Name;
        entity.IsActive = dto.IsActive;

        foreach (var rowDto in dto.RowPrices)
        {
            var rowEntity = entity.RowPrices.FirstOrDefault(x => x.Row == rowDto.Row);
            if (rowEntity != null)
            {
                rowEntity.BasePrice = rowDto.BasePrice;
            }
        }

        foreach (var catDto in dto.CategoryMultipliers)
        {
            var catEntity = entity.CategoryMultipliers
                .FirstOrDefault(x => (int)x.Category == catDto.Category);

            if (catEntity != null)
            {
                catEntity.Multiplier = catDto.Multiplier;
            }
        }

        await _repo.UpdateAsync(entity, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity != null)
        {
            await _repo.DeleteAsync(entity, ct);
        }
    }
}