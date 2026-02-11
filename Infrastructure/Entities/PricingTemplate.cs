namespace Infrastructure.Entities;

public class PricingTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int? HallId { get; set; }
    public ICollection<PricingTemplateRowPrice> RowPrices { get; set; } = new List<PricingTemplateRowPrice>();
    public ICollection<PricingTemplateCategoryMultiplier> CategoryMultipliers { get; set; } = new List<PricingTemplateCategoryMultiplier>();
}