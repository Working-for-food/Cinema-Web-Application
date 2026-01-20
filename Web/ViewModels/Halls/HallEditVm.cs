using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Halls;

public class HallEditVm
{
    public int? Id { get; set; }

    [Required]
    public int CinemaId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = "";
}
