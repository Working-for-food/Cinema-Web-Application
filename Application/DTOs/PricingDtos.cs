namespace Application.DTOs.Pricing;

public sealed record SeatMapItemDto
{
    public int SeatId { get; init; }
    public int Row { get; init; }
    public int Number { get; init; }
    public int Category { get; init; }
}

public sealed record RowPriceDto
{
    public int Row { get; init; }
    public decimal BasePrice { get; init; }
}

public sealed record CategoryMultiplierDto
{
    public int Category { get; init; }
    public decimal Multiplier { get; init; }
}

public sealed record SessionPricingDto
{
    public int SessionId { get; init; }
    public int HallId { get; init; }
    public List<RowPriceDto> RowPrices { get; init; } = new();
    public List<CategoryMultiplierDto> CategoryMultipliers { get; init; } = new();
}

public sealed record SessionSeatPriceDto
{
    public int SeatId { get; init; }
    public int Row { get; init; }
    public int Number { get; init; }
    public int Category { get; init; }
    public decimal Price { get; init; }
}

public sealed record HallPricingMetaDto(
    IReadOnlyList<int> Rows,
    IReadOnlyList<CategoryItemDto> Categories);

public sealed record CategoryItemDto(int Id, string Title);

public sealed record PricingTemplateListItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int? HallId { get; init; }
}

public sealed record PricingTemplateEditDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public int? HallId { get; init; }

    public List<RowPriceDto> RowPrices { get; init; } = new();
    public List<CategoryMultiplierDto> CategoryMultipliers { get; init; } = new();
}

public sealed record ApplyPricingTemplateResultDto
{
    public int PricingTemplateId { get; init; }
    public List<RowPriceDto> RowPrices { get; init; } = new();
    public List<CategoryMultiplierDto> CategoryMultipliers { get; init; } = new();
}
