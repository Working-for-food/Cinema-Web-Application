namespace Infrastructure.Entities;

public class PricingTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int? HallId { get; set; }
}