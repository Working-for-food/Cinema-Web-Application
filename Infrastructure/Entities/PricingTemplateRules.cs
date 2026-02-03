namespace Infrastructure.Entities;

public class PricingTemplateRowPrice
{
    public int PricingTemplateId { get; set; }
    public int Row { get; set; }
    public decimal BasePrice { get; set; }

    public PricingTemplate PricingTemplate { get; set; } = null!;
}

public class PricingTemplateCategoryMultiplier
{
    public int PricingTemplateId { get; set; }
    public SeatCategory Category { get; set; }
    public decimal Multiplier { get; set; }

    public PricingTemplate PricingTemplate { get; set; } = null!;
}