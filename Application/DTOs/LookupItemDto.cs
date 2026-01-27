using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs;

public record LookupItemDto(int Id, string Title, int? DurationMinutes = null);
