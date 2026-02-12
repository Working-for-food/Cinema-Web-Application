using Application.DTOs;
namespace Application.Interfaces;

public interface IScheduleService
{
    Task<CinemaScheduleDto> GetCinemaScheduleAsync(int cinemaId, DateOnly? selectedDate, CancellationToken ct);
}
