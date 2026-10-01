using BridgeTask.Api.Common.Pagination;

namespace BridgeTask.Api.Dtos.Countries;

public sealed record CreateCountryRequest(string Name, string Code);

public sealed record UpdateCountryRequest(string Name, string Code);

public sealed record CountryResponse(
    int Id,
    string Name,
    string Code,
    int CitiesCount,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CountryQuery : PagedQuery
{
    public static readonly string[] SortableFields = ["name", "code", "createdAt"];

    /// Case-insensitive partial match on name or code.
    public string? Search { get; init; }

    /// Exact match on the ISO code.
    public string? Code { get; init; }
}
