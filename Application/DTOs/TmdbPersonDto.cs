using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs;
public sealed class TmdbPersonDto
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public string? Biography { get; init; }
    public string? ProfilePath { get; init; }
    public string? Birthday { get; init; }
}
