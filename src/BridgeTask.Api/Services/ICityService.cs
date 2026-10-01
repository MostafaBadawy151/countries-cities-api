using BridgeTask.Api.Common.Pagination;
using BridgeTask.Api.Dtos.Cities;

namespace BridgeTask.Api.Services;

public interface ICityService
{
    Task<CityResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<PagedResult<CityResponse>> GetPagedAsync(CityQuery query, CancellationToken ct);
    Task<PagedResult<CityResponse>> GetByCountryAsync(int countryId, CountryCitiesQuery query, CancellationToken ct);
    Task<CityResponse> CreateAsync(CreateCityRequest request, CancellationToken ct);
    Task<CityResponse> UpdateAsync(int id, UpdateCityRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
