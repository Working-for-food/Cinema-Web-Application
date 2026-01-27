using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface ITestHallRepository
{
    Task<List<Hall>> GetAllWithCinemaAsync(CancellationToken ct);
}
