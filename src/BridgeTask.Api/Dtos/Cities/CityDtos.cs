using BridgeTask.Api.Common.Pagination;

namespace BridgeTask.Api.Dtos.Cities;

public sealed record CreateCityRequest(string Name, int CountryId, long? Population);

public sealed record UpdateCityRequest(string Name, int CountryId, long? Population);

public sealed record CityResponse(
    int Id,
    string Name,
    long? Population,
    int CountryId,
    string CountryName,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public record CityQuery : PagedQuery
{
    public static readonly string[] SortableFields = ["name", "population", "country", "createdAt"];

    /// Case-insensitive partial match on the city name.
    public string? Search { get; init; }

    public int? CountryId { get; init; }
    public long? MinPopulation { get; init; }
    public long? MaxPopulation { get; init; }
}

/// Query for the nested /countries/{id}/cities route; the country comes from the route.
public sealed record CountryCitiesQuery : PagedQuery
{
    public string? Search { get; init; }
}
