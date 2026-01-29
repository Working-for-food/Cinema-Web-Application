using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Admin.Halls;

public class HallEditVm
{
    public int? Id { get; set; }

    [Required]
    public int CinemaId { get; set; }

    [Required(ErrorMessage = "Назва є обов’язковою")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Назва має містити від 3 до 100 символів")]
    public string Name { get; set; } = "";

    public List<SelectListItem> Cinemas { get; set; } = new();
}