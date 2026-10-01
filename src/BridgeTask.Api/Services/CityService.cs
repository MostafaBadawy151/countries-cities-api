using System.Linq.Expressions;
using BridgeTask.Api.Common.Exceptions;
using BridgeTask.Api.Common.Pagination;
using BridgeTask.Api.Data;
using BridgeTask.Api.Dtos.Cities;
using BridgeTask.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BridgeTask.Api.Services;

public sealed class CityService(AppDbContext db, ILogger<CityService> logger) : ICityService
{
    private static readonly Expression<Func<City, CityResponse>> ToResponse = c =>
        new CityResponse(c.Id, c.Name, c.Population, c.CountryId, c.Country.Name, c.CreatedAtUtc, c.UpdatedAtUtc);

    public async Task<CityResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Cities
                   .AsNoTracking()
                   .Where(c => c.Id == id)
                   .Select(ToResponse)
                   .FirstOrDefaultAsync(ct)
               ?? throw new NotFoundException(nameof(City), id);
    }

    public Task<PagedResult<CityResponse>> GetPagedAsync(CityQuery query, CancellationToken ct)
    {
        var cities = ApplySearch(db.Cities.AsNoTracking(), query.Search);

        if (query.CountryId.HasValue)
            cities = cities.Where(c => c.CountryId == query.CountryId);

        if (query.MinPopulation.HasValue)
            cities = cities.Where(c => c.Population >= query.MinPopulation);

        if (query.MaxPopulation.HasValue)
            cities = cities.Where(c => c.Population <= query.MaxPopulation);

        return ApplySorting(cities, query)
            .Select(ToResponse)
            .ToPagedResultAsync(query.PageNumber, query.PageSize, ct);
    }

    public async Task<PagedResult<CityResponse>> GetByCountryAsync(
        int countryId, CountryCitiesQuery query, CancellationToken ct)
    {
        await EnsureCountryExistsAsync(countryId, ct);

        var cities = ApplySearch(db.Cities.AsNoTracking().Where(c => c.CountryId == countryId), query.Search);

        return await ApplySorting(cities, query)
            .Select(ToResponse)
            .ToPagedResultAsync(query.PageNumber, query.PageSize, ct);
    }

    public async Task<CityResponse> CreateAsync(CreateCityRequest request, CancellationToken ct)
    {
        var name = request.Name.Trim();

        await EnsureCountryExistsAsync(request.CountryId, ct);
        await EnsureUniqueAsync(name, request.CountryId, excludeId: null, ct);

        var city = new City { Name = name, CountryId = request.CountryId, Population = request.Population };
        db.Cities.Add(city);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created city {CityId} in country {CountryId}", city.Id, city.CountryId);
        return await GetByIdAsync(city.Id, ct);
    }

    public async Task<CityResponse> UpdateAsync(int id, UpdateCityRequest request, CancellationToken ct)
    {
        var city = await db.Cities.FindAsync([id], ct)
                   ?? throw new NotFoundException(nameof(City), id);

        var name = request.Name.Trim();

        if (city.CountryId != request.CountryId)
            await EnsureCountryExistsAsync(request.CountryId, ct);

        await EnsureUniqueAsync(name, request.CountryId, excludeId: id, ct);

        city.Name = name;
        city.CountryId = request.CountryId;
        city.Population = request.Population;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated city {CityId}", id);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var city = await db.Cities.FindAsync([id], ct)
                   ?? throw new NotFoundException(nameof(City), id);

        db.Cities.Remove(city);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deleted city {CityId}", id);
    }

    private async Task EnsureCountryExistsAsync(int countryId, CancellationToken ct)
    {
        if (!await db.Countries.AnyAsync(c => c.Id == countryId, ct))
            throw new NotFoundException(nameof(Country), countryId);
    }

    private async Task EnsureUniqueAsync(string name, int countryId, int? excludeId, CancellationToken ct)
    {
        var exists = await db.Cities.AnyAsync(c =>
            c.CountryId == countryId &&
            c.Name == name &&
            (excludeId == null || c.Id != excludeId), ct);

        if (exists)
            throw new ConflictException($"A city named '{name}' already exists in this country.");
    }

    private static IQueryable<City> ApplySearch(IQueryable<City> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var pattern = SearchPattern.Contains(search);
        return query.Where(c => EF.Functions.Like(c.Name, pattern, SearchPattern.EscapeCharacter));
    }

    private static IQueryable<City> ApplySorting(IQueryable<City> query, PagedQuery q)
    {
        var desc = q.SortDirection == SortDirection.Desc;

        IOrderedQueryable<City> ordered = q.SortBy?.ToLowerInvariant() switch
        {
            "population" => desc ? query.OrderByDescending(c => c.Population) : query.OrderBy(c => c.Population),
            "country" => desc ? query.OrderByDescending(c => c.Country.Name) : query.OrderBy(c => c.Country.Name),
            "createdat" => desc ? query.OrderByDescending(c => c.CreatedAtUtc) : query.OrderBy(c => c.CreatedAtUtc),
            _ => desc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name)
        };

        return ordered.ThenBy(c => c.Id);
    }
}
