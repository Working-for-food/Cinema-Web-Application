using Application.DTOs.Afisha;

namespace Application.Interfaces;

public interface IAfishaService
{
    Task<AfishaIndexDto> GetAfishaAsync(CancellationToken ct = default);
}
