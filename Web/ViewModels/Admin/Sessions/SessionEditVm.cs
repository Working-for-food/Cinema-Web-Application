using Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Admin.Sessions;

public class SessionEditVm : IValidatableObject
{
    public int? Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Оберіть кінотеатр.")]
    public int CinemaId { get; set; } = 0;

    [Range(1, int.MaxValue, ErrorMessage = "Оберіть фільм.")]
    public int MovieId { get; set; } = 0;

    [Range(1, int.MaxValue, ErrorMessage = "Оберіть зал.")]
    public int HallId { get; set; } = 0;

    [Required(ErrorMessage = "Час початку є обов’язковим.")]
    [DataType(DataType.DateTime)]
    public DateTime? StartTime { get; set; }

    [Required(ErrorMessage = "Час завершення є обов’язковим.")]
    [DataType(DataType.DateTime)]
    public DateTime? EndTime { get; set; }

    [Required(ErrorMessage = "Тип показу є обов’язковим.")]
    public PresentationType PresentationType { get; set; }

    public List<SelectListItem> Cinemas { get; set; } = new();
    public List<SelectListItem> Movies { get; set; } = new();
    public List<SelectListItem> Halls { get; set; } = new();
    public List<SelectListItem> PresentationTypes { get; set; } = new();

    public List<RowPriceVm> RowPrices { get; set; } = new();
    public List<CategoryMultiplierVm> CategoryMultipliers { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartTime.HasValue && EndTime.HasValue && StartTime.Value >= EndTime.Value)
        {
            yield return new ValidationResult(
                "Час початку має бути раніше часу завершення.",
                new[] { nameof(StartTime), nameof(EndTime) });
        }

        if (HallId <= 0)
            yield break;

        if (RowPrices.Count == 0)
            yield return new ValidationResult("Заповніть ціни по рядах.", new[] { nameof(RowPrices) });

        if (CategoryMultipliers.Count == 0)
            yield return new ValidationResult("Заповніть множники по категоріях.", new[] { nameof(CategoryMultipliers) });

        if (RowPrices.Count > 0)
        {
            if (RowPrices.Any(x => x.RowNumber <= 0))
                yield return new ValidationResult("Некоректний номер ряду у цінах.", new[] { nameof(RowPrices) });

            if (RowPrices.Any(x => x.BasePrice <= 0))
                yield return new ValidationResult("Ціна ряду має бути > 0.", new[] { nameof(RowPrices) });

            if (RowPrices.Select(x => x.RowNumber).Distinct().Count() != RowPrices.Count)
                yield return new ValidationResult("Ряди в цінах дублюються.", new[] { nameof(RowPrices) });
        }

        if (CategoryMultipliers.Count > 0)
        {
            if (CategoryMultipliers.Any(x => x.Multiplier <= 0))
                yield return new ValidationResult("Множник категорії має бути > 0.", new[] { nameof(CategoryMultipliers) });

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
    }
}
