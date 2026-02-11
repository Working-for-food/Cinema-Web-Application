using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Entities;

public class SavedMovie
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    public int MovieId { get; set; }
    public Movie Movie { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
