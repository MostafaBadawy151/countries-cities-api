using BridgeTask.Api.Common.Pagination;
using BridgeTask.Api.Dtos.Countries;

namespace BridgeTask.Api.Services;

public interface ICountryService
{
    Task<CountryResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<PagedResult<CountryResponse>> GetPagedAsync(CountryQuery query, CancellationToken ct);
    Task<CountryResponse> CreateAsync(CreateCountryRequest request, CancellationToken ct);
    Task<CountryResponse> UpdateAsync(int id, UpdateCountryRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
