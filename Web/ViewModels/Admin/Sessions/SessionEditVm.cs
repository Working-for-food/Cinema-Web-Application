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
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartTime.HasValue && EndTime.HasValue && StartTime.Value >= EndTime.Value)
        {
            yield return new ValidationResult(
                "Час початку має бути раніше часу завершення.",
                new[] { nameof(StartTime), nameof(EndTime) });
        }
    }
}
