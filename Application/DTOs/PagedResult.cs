namespace Application.DTOs;

public class PagedResult<T>
{
    public required List<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }

    public int TotalPages => TotalCount <= 0
        ? 1
        : (int)Math.Ceiling((double)TotalCount / PageSize);
}
