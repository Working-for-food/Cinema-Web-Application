using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CinemaEditDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2–100 characters")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Address is required")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Address must be 3–150 characters")]
    public string Address { get; set; } = "";

    [Required(ErrorMessage = "City is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "City must be 2–50 characters")]
    public string City { get; set; } = "";
}

