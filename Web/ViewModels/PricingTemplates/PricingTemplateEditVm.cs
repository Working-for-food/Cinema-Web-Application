using System.ComponentModel.DataAnnotations;
using Application.DTOs.Pricing;

namespace Web.ViewModels.Admin.PricingTemplates;

public class PricingTemplateEditVm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Вкажіть назву шаблону")]
    [Display(Name = "Назва шаблону")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Активний шаблон")]
    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "Оберіть зал")]
    [Range(1, int.MaxValue, ErrorMessage = "Оберіть зал")]
    [Display(Name = "Зал")]
    public int? HallId { get; set; }

    public List<RowPriceDto> RowPrices { get; set; } = new();
    public List<CategoryMultiplierDto> CategoryMultipliers { get; set; } = new();
}