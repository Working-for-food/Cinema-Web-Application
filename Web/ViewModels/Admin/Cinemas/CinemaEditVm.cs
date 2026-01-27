using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Admin.Cinemas;

public class CinemaEditVm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Назва є обов’язковою")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва має містити від 2 до 100 символів")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Адреса є обов’язковою")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Адреса має містити від 3 до 150 символів")]
    public string Address { get; set; } = "";

    [Required(ErrorMessage = "Місто є обов’язковим")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Назва міста має містити від 2 до 50 символів")]
    public string City { get; set; } = "";
}
