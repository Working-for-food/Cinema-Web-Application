using Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record SessionEditDto : IValidatableObject
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int MovieId { get; init; }

        [Required]
        [Range(1, int.MaxValue)]
        public int HallId { get; init; }

        [Required]
        public DateTime StartTime { get; init; }

        [Required]
        public DateTime EndTime { get; init; }

        [Required]
        public PresentationType PresentationType { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartTime >= EndTime)
            {
                yield return new ValidationResult(
                    "StartTime має бути раніше EndTime.",
                    new[] { nameof(StartTime), nameof(EndTime) });
            }
        }
    }
}
