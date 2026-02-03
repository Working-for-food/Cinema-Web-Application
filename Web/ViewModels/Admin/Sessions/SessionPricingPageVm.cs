using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Admin.Sessions;

public sealed class SessionPricingPageVm : IValidatableObject
{
    public int SessionId { get; set; }

    public string MovieTitle { get; set; } = "";
    public string CinemaName { get; set; } = "";
    public string HallName { get; set; } = "";

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public List<RowPriceVm> RowPrices { get; set; } = new();
    public List<CategoryMultiplierVm> CategoryMultipliers { get; set; } = new();

    public List<SeatPriceVm> Seats { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RowPrices is null || RowPrices.Count == 0)
            yield return new ValidationResult("Заповніть ціни по рядах.", new[] { nameof(RowPrices) });

        if (CategoryMultipliers is null || CategoryMultipliers.Count == 0)
            yield return new ValidationResult("Заповніть множники по категоріях.", new[] { nameof(CategoryMultipliers) });

        if (RowPrices is not null)
        {
            if (RowPrices.Any(x => x.RowNumber <= 0))
                yield return new ValidationResult("Некоректний номер ряду.", new[] { nameof(RowPrices) });

            if (RowPrices.Any(x => x.BasePrice <= 0))
                yield return new ValidationResult("Ціна ряду має бути > 0.", new[] { nameof(RowPrices) });

            if (RowPrices.Select(x => x.RowNumber).Distinct().Count() != RowPrices.Count)
                yield return new ValidationResult("Ряди в цінах дублюються.", new[] { nameof(RowPrices) });
        }

        if (CategoryMultipliers is not null)
        {
            if (CategoryMultipliers.Any(x => x.Category < 0))
                yield return new ValidationResult("Некоректна категорія.", new[] { nameof(CategoryMultipliers) });

            if (CategoryMultipliers.Any(x => x.Multiplier <= 0))
                yield return new ValidationResult("Множник має бути > 0.", new[] { nameof(CategoryMultipliers) });

            if (CategoryMultipliers.Select(x => x.Category).Distinct().Count() != CategoryMultipliers.Count)
                yield return new ValidationResult("Категорії в множниках дублюються.", new[] { nameof(CategoryMultipliers) });
        }
    }

    public sealed class RowPriceVm
    {
        public int RowNumber { get; set; }
        public decimal BasePrice { get; set; }
    }

    public sealed class CategoryMultiplierVm
    {
        public int Category { get; set; }
        public decimal Multiplier { get; set; }
        public string Title { get; set; } = "";
    }

    public sealed class SeatPriceVm
    {
        public int SeatId { get; set; }
        public int Row { get; set; }
        public int Number { get; set; }
        public int Category { get; set; }
        public decimal Price { get; set; }
    }
}