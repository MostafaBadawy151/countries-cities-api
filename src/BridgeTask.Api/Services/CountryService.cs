using System.Linq.Expressions;
using BridgeTask.Api.Common.Exceptions;
using BridgeTask.Api.Common.Pagination;
using BridgeTask.Api.Data;
using BridgeTask.Api.Dtos.Countries;
using BridgeTask.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BridgeTask.Api.Services;

public sealed class CountryService(AppDbContext db, ILogger<CountryService> logger) : ICountryService
{
    private static readonly Expression<Func<Country, CountryResponse>> ToResponse = c =>
        new CountryResponse(c.Id, c.Name, c.Code, c.Cities.Count, c.CreatedAtUtc, c.UpdatedAtUtc);

    public async Task<CountryResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Countries
                   .AsNoTracking()
                   .Where(c => c.Id == id)
                   .Select(ToResponse)
                   .FirstOrDefaultAsync(ct)
               ?? throw new NotFoundException(nameof(Country), id);
    }

    public Task<PagedResult<CountryResponse>> GetPagedAsync(CountryQuery query, CancellationToken ct)
    {
        var countries = db.Countries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = SearchPattern.Contains(query.Search);
            countries = countries.Where(c =>
                EF.Functions.Like(c.Name, pattern, SearchPattern.EscapeCharacter) ||
                EF.Functions.Like(c.Code, pattern, SearchPattern.EscapeCharacter));
        }

        if (!string.IsNullOrWhiteSpace(query.Code))
        {
            var code = NormalizeCode(query.Code);
            countries = countries.Where(c => c.Code == code);
        }

        return ApplySorting(countries, query)
            .Select(ToResponse)
            .ToPagedResultAsync(query.PageNumber, query.PageSize, ct);
    }

    public async Task<CountryResponse> CreateAsync(CreateCountryRequest request, CancellationToken ct)
    {
        var name = request.Name.Trim();
        var code = NormalizeCode(request.Code);

        await EnsureUniqueAsync(name, code, excludeId: null, ct);

        var country = new Country { Name = name, Code = code };
        db.Countries.Add(country);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created country {CountryId} ({Code})", country.Id, country.Code);
        return await GetByIdAsync(country.Id, ct);
    }

    public async Task<CountryResponse> UpdateAsync(int id, UpdateCountryRequest request, CancellationToken ct)
    {
        var country = await db.Countries.FindAsync([id], ct)
                      ?? throw new NotFoundException(nameof(Country), id);

        var name = request.Name.Trim();
        var code = NormalizeCode(request.Code);

        await EnsureUniqueAsync(name, code, excludeId: id, ct);

        country.Name = name;
        country.Code = code;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated country {CountryId}", id);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var country = await db.Countries.FindAsync([id], ct)
                      ?? throw new NotFoundException(nameof(Country), id);

        if (await db.Cities.AnyAsync(c => c.CountryId == id, ct))
            throw new ConflictException(
                $"Country '{country.Name}' cannot be deleted because it still has cities. Delete its cities first.");

        db.Countries.Remove(country);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deleted country {CountryId}", id);
    }

    private async Task EnsureUniqueAsync(string name, string code, int? excludeId, CancellationToken ct)
    {
        // Name and Code columns use NOCASE collation, so these comparisons are case-insensitive.
        var others = db.Countries.Where(c => excludeId == null || c.Id != excludeId);

        if (await others.AnyAsync(c => c.Name == name, ct))
            throw new ConflictException($"A country named '{name}' already exists.");

        if (await others.AnyAsync(c => c.Code == code, ct))
            throw new ConflictException($"A country with code '{code}' already exists.");
    }

    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private static IQueryable<Country> ApplySorting(IQueryable<Country> query, CountryQuery q)
    {
        var desc = q.SortDirection == SortDirection.Desc;

        IOrderedQueryable<Country> ordered = q.SortBy?.ToLowerInvariant() switch
        {
            "code" => desc ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "createdat" => desc ? query.OrderByDescending(c => c.CreatedAtUtc) : query.OrderBy(c => c.CreatedAtUtc),
            _ => desc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name)
        };

        // Tie-breaker keeps paging stable.
        return ordered.ThenBy(c => c.Id);
    }
}
