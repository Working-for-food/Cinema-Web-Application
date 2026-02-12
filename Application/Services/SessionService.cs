using Application.DTOs;
using Application.Interfaces;
using Application.DTOs.Pricing;
using Infrastructure.Entities;
using Infrastructure.Interfaces;

namespace Application.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _repo;
    private readonly IMovieRepository _movieRepo;
    private readonly ISessionPricingRepository _pricingRepo;
    private const int PageSize = 20;

    public SessionService(
            ISessionRepository repo,
            ISessionPricingRepository pricingRepo,
            IMovieRepository movieRepo)
    {
        _repo = repo;
        _pricingRepo = pricingRepo;
        _movieRepo = movieRepo;
    }

    public async Task<SessionDetailsDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var s = await _repo.GetByIdAsync(id, ct);
        if (s is null) return null;

        return new SessionDetailsDto
        {
            Id = s.Id,
            MovieId = s.MovieId,
            MovieTitle = s.Movie?.Title ?? "",

            HallName = s.Hall?.Name ?? "",
            HallId = s.HallId,

            CinemaName = s.Hall?.Cinema?.Name ?? "",
            CinemaId = s.Hall?.CinemaId ?? 0,

            StartTime = s.StartTime,
            EndTime = s.EndTime,
            PresentationType = s.PresentationType,
            IsCancelled = s.IsCancelled,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
    }

    public async Task<IReadOnlyList<SessionSeatDto>> GetSeatsForBookingAsync(int sessionId, CancellationToken ct)
    {
        var s = await _repo.GetByIdAsync(sessionId, ct);
        if (s is null) return Array.Empty<SessionSeatDto>();

        await _pricingRepo.EnsureSessionSeatsCreatedAsync(sessionId, s.HallId, ct);

        var seats = await _pricingRepo.GetSessionSeatsWithSeatAsync(sessionId, ct);

        return seats
            .OrderBy(x => x.Seat.RowNumber).ThenBy(x => x.Seat.SeatNumber)
            .Select(x => new SessionSeatDto
            {
                SeatId = x.SeatId,
                RowNumber = x.Seat.RowNumber,
                SeatNumber = x.Seat.SeatNumber,
                Category = (int)x.Seat.Category,
                Price = x.Price,
                BookingId = x.BookingId
            })
            .ToList();
    }

    public async Task<SessionEditDto?> GetForEditAsync(int id, CancellationToken ct)
    {
        var s = await _repo.GetByIdAsync(id, ct);
        if (s is null) return null;

        return new SessionEditDto
        {
            MovieId = s.MovieId,
            HallId = s.HallId,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            PresentationType = s.PresentationType
        };
    }

    public async Task<PagedResult<SessionListDto>> GetAllPagedAsync(
        DateTime? from,
        DateTime? to,
        int? cinemaId,
        int? hallId,
        int? movieId,
        bool includeCancelled,
        bool includeFinished,
        string? sort,
        int page,
        CancellationToken ct)
    {
        if (page < 1) page = 1;

        var fromNorm = from?.Date;
        var toExclusive = to?.Date.AddDays(1);

        var asOf = DateTime.Now;

        var (items, totalCount) = await _repo.GetAllPagedAsync(
            fromNorm,
            toExclusive,
            cinemaId,
            hallId,
            movieId,
            includeCancelled,
            includeFinished,
            asOf,
            sort,
            page,
            PageSize,
            ct);

        var dtos = items.Select(s => new SessionListDto
        {
            Id = s.Id,
            MovieId = s.MovieId,
            HallId = s.HallId,

            MovieTitle = s.Movie?.Title,
            PosterPath = s.Movie?.PosterPath,
            HallName = s.Hall?.Name,
            CinemaName = s.Hall?.Cinema?.Name,

            StartTime = s.StartTime,
            EndTime = s.EndTime,
            PresentationType = s.PresentationType,
            IsCancelled = s.IsCancelled
        }).ToList();

        return new PagedResult<SessionListDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = PageSize
        };
    }

    public async Task<int> CreateAsync(SessionEditDto dto, CancellationToken ct)
    {
        ValidateTimeRange(dto.StartTime, dto.EndTime);

        await ValidateReleaseDateAsync(dto.MovieId, dto.StartTime, ct);

        var hasOverlap = await _repo.HasOverlapAsync(dto.HallId, dto.StartTime, dto.EndTime, ignoreSessionId: null, ct);
        if (hasOverlap)
            throw new InvalidOperationException("У цьому залі вже є сеанс, що перетинається за часом.");

        var entity = new Session
        {
            MovieId = dto.MovieId,
            HallId = dto.HallId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            PresentationType = dto.PresentationType,
            IsCancelled = false,
            CreatedAt = DateTime.Now,
            UpdatedAt = null
        };

        await _repo.AddAsync(entity, ct);
        await _pricingRepo.EnsureSessionSeatsCreatedAsync(entity.Id, dto.HallId, ct);

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(int id, SessionEditDto dto, CancellationToken ct)
    {
        ValidateTimeRange(dto.StartTime, dto.EndTime);

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return false;

        if (entity.IsCancelled)
            throw new InvalidOperationException("Не можна редагувати скасований сеанс. Спочатку відновіть його.");

        EnsureNotOver(entity, "редагувати сеанс");

        if (entity.MovieId != dto.MovieId || entity.StartTime.Date != dto.StartTime.Date)
        {
            await ValidateReleaseDateAsync(dto.MovieId, dto.StartTime, ct);
        }

        var hasOverlap = await _repo.HasOverlapAsync(dto.HallId, dto.StartTime, dto.EndTime, ignoreSessionId: id, ct);
        if (hasOverlap)
            throw new InvalidOperationException("У цьому залі вже є сеанс, що перетинається за часом.");

        if (dto.HallId != entity.HallId)
            throw new InvalidOperationException("Зал сеансу змінювати не можна. Створіть новий сеанс у потрібному залі");

        entity.MovieId = dto.MovieId;
        entity.StartTime = dto.StartTime;
        entity.EndTime = dto.EndTime;
        entity.PresentationType = dto.PresentationType;
        entity.UpdatedAt = DateTime.Now;

        await _repo.UpdateAsync(entity, ct);

        return true;
    }

    public async Task<bool> CancelAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return false;

        EnsureNotOver(entity, "скасувати сеанс");

        if (!entity.IsCancelled)
        {
            entity.IsCancelled = true;
            entity.UpdatedAt = DateTime.Now;
            await _repo.UpdateAsync(entity, ct);
        }

        return true;
    }

    public async Task<bool> RestoreAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return false;

        EnsureNotOver(entity, "відновити сеанс");

        if (entity.IsCancelled)
        {
            var hasOverlap = await _repo.HasOverlapAsync(entity.HallId, entity.StartTime, entity.EndTime, ignoreSessionId: id, ct);
            if (hasOverlap)
                throw new InvalidOperationException("Неможливо відновити сеанс: у цьому залі вже є інший сеанс, що перетинається за часом.");

            entity.IsCancelled = false;
            entity.UpdatedAt = DateTime.Now;

            await _repo.UpdateAsync(entity, ct);
        }

        return true;
    }

    private async Task ValidateReleaseDateAsync(int movieId, DateTime sessionStart, CancellationToken ct)
    {
        var movie = await _movieRepo.GetByIdAsync(movieId, ct);
        if (movie == null)
            throw new InvalidOperationException("Фільм не знайдено.");

        var sessionDate = DateOnly.FromDateTime(sessionStart);

        if (sessionDate < movie.ReleaseDate)
        {
            throw new InvalidOperationException(
                $"Неможливо створити сеанс на {sessionDate:dd.MM.yyyy}. " +
                $"Офіційна прем'єра фільму '{movie.Title}': {movie.ReleaseDate:dd.MM.yyyy}.");
        }
    }

    private static void ValidateTimeRange(DateTime start, DateTime end)
    {
        if (start >= end)
            throw new ArgumentException("Час початку має бути раніше часу завершення.");
    }

    private static bool IsOver(DateTime endTime)
    {
        return endTime <= DateTime.Now;
    }

    private static void EnsureNotOver(Session s, string action)
    {
        if (IsOver(s.EndTime))
            throw new InvalidOperationException($"Не можна {action}: сеанс уже завершився.");
    }

    public async Task EnsureSessionSeatsCreatedAsync(int sessionId, CancellationToken ct)
    {
        var s = await _repo.GetByIdAsync(sessionId, ct);
        if (s is null) return;

        await _pricingRepo.EnsureSessionSeatsCreatedAsync(sessionId, s.HallId, ct);
    }

    public async Task<bool> HasBookingsAsync(int sessionId, CancellationToken ct)
    {
        return await _repo.HasBookingsAsync(sessionId, ct);
    }

    public async Task<SessionPricingDto> GetPricingAsync(int sessionId, CancellationToken ct)
    {
        var s = await _repo.GetByIdAsync(sessionId, ct);
        if (s is null)
            throw new InvalidOperationException("Сеанс не знайдено.");

        var hallSeats = await _pricingRepo.GetHallSeatsAsync(s.HallId, ct);

        var hallRows = hallSeats
            .Select(x => x.RowNumber)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var hallCats = hallSeats
            .Select(x => (int)x.Category)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var existingRows = await _pricingRepo.GetRowPricesAsync(sessionId, ct);
        var existingCats = await _pricingRepo.GetCategoryMultipliersAsync(sessionId, ct);

        var rowPrices = existingRows.Count > 0
            ? existingRows
                .Where(x => hallRows.Contains(x.RowNumber))
                .Select(x => new RowPriceDto { Row = x.RowNumber, BasePrice = x.BasePrice })
                .ToList()
            : hallRows.Select(r => new RowPriceDto { Row = r, BasePrice = 100m }).ToList();

        var multipliers = existingCats.Count > 0
            ? existingCats
                .Where(x => hallCats.Contains((int)x.Category))
                .Select(x => new CategoryMultiplierDto { Category = (int)x.Category, Multiplier = x.Multiplier })
                .ToList()
            : hallCats.Select(c => new CategoryMultiplierDto { Category = c, Multiplier = 1m }).ToList();

        var rowSet = rowPrices.Select(x => x.Row).ToHashSet();
        foreach (var r in hallRows.Where(r => !rowSet.Contains(r)))
            rowPrices.Add(new RowPriceDto { Row = r, BasePrice = 100m });

        var catSet = multipliers.Select(x => x.Category).ToHashSet();
        foreach (var c in hallCats.Where(c => !catSet.Contains(c)))
            multipliers.Add(new CategoryMultiplierDto { Category = c, Multiplier = 1m });

        rowPrices = rowPrices.OrderBy(x => x.Row).ToList();
        multipliers = multipliers.OrderBy(x => x.Category).ToList();

        return new SessionPricingDto
        {
            SessionId = sessionId,
            HallId = s.HallId,
            RowPrices = rowPrices,
            CategoryMultipliers = multipliers
        };
    }

    public async Task ApplyPricingAsync(int sessionId, SessionPricingDto pricing, CancellationToken ct)
    {
        var s = await _repo.GetByIdAsync(sessionId, ct);
        if (s is null)
            throw new InvalidOperationException("Сеанс не знайдено.");

        await _pricingRepo.EnsureSessionSeatsCreatedAsync(sessionId, s.HallId, ct);

        var hallSeats = await _pricingRepo.GetHallSeatsAsync(s.HallId, ct);

        var hallRowSet = hallSeats.Select(x => x.RowNumber).ToHashSet();
        var hallCatSet = hallSeats.Select(x => (int)x.Category).ToHashSet();

        var rowPrices = pricing.RowPrices ?? new List<RowPriceDto>();
        var catMultipliers = pricing.CategoryMultipliers ?? new List<CategoryMultiplierDto>();

        if (rowPrices.Count == 0)
            throw new InvalidOperationException("Заповніть ціни по рядах.");
        if (catMultipliers.Count == 0)
            throw new InvalidOperationException("Заповніть множники по категоріях.");
        if (rowPrices.Any(x => x.BasePrice <= 0))
            throw new InvalidOperationException("Ціни по рядах мають бути > 0.");
        if (catMultipliers.Any(x => x.Multiplier <= 0))
            throw new InvalidOperationException("Множники мають бути > 0.");

        var validRowPrices = rowPrices
            .Where(x => hallRowSet.Contains(x.Row))
            .DistinctBy(x => x.Row) 
            .Select(x => new SessionRowPrice
            {
                SessionId = sessionId,
                RowNumber = x.Row,
                BasePrice = x.BasePrice
            })
            .ToList();

        var validMultipliers = catMultipliers
            .Where(x => hallCatSet.Contains(x.Category))
            .DistinctBy(x => x.Category)
            .Select(x => new SessionCategoryMultiplier
            {
                SessionId = sessionId,
                Category = (SeatCategory)x.Category,
                Multiplier = x.Multiplier
            })
            .ToList();

        await _pricingRepo.ReplaceRowPricesAsync(sessionId, validRowPrices, ct);
        await _pricingRepo.ReplaceCategoryMultipliersAsync(sessionId, validMultipliers, ct);

        var rowMap = validRowPrices.ToDictionary(x => x.RowNumber, x => x.BasePrice);
        var catMap = validMultipliers.ToDictionary(x => (int)x.Category, x => x.Multiplier);

        var sessionSeats = await _pricingRepo.GetSessionSeatsWithSeatAsync(sessionId, ct);

        var seatIdToPrice = new Dictionary<int, decimal>();

        foreach (var ss in sessionSeats)
        {
            if (rowMap.TryGetValue(ss.Seat.RowNumber, out var basePrice) &&
                catMap.TryGetValue((int)ss.Seat.Category, out var mult))
            {
                var finalPrice = Math.Round(basePrice * mult, 2, MidpointRounding.AwayFromZero);
                seatIdToPrice[ss.SeatId] = finalPrice;
            }
        }

        await _pricingRepo.UpdateSessionSeatPricesAsync(sessionId, seatIdToPrice, ct);
    }

    public async Task<IReadOnlyList<SessionSeatPriceDto>> GetSeatPricesAsync(int sessionId, CancellationToken ct)
    {
        var s = await _repo.GetByIdAsync(sessionId, ct);
        if (s is null) return Array.Empty<SessionSeatPriceDto>();

        await _pricingRepo.EnsureSessionSeatsCreatedAsync(sessionId, s.HallId, ct);

        var items = await _pricingRepo.GetSessionSeatsWithSeatAsync(sessionId, ct);

        return items
            .OrderBy(x => x.Seat.RowNumber).ThenBy(x => x.Seat.SeatNumber)
            .Select(x => new SessionSeatPriceDto
            {
                SeatId = x.SeatId,
                Row = x.Seat.RowNumber,
                Number = x.Seat.SeatNumber,
                Category = (int)x.Seat.Category,
                Price = x.Price
            })
            .ToList();
    }
}
