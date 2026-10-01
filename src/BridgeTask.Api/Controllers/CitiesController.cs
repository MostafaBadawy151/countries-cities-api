using BridgeTask.Api.Common.Pagination;
using BridgeTask.Api.Dtos.Cities;
using BridgeTask.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BridgeTask.Api.Controllers;

[ApiController]
[Route("api/cities")]
[Produces("application/json")]
public sealed class CitiesController(ICityService cities) : ControllerBase
{
    /// Gets cities with pagination, filtering and sorting.
    /// Sortable fields: name, population, country, createdAt.
    [HttpGet]
    [ProducesResponseType<PagedResult<CityResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CityResponse>>> GetCities(
        [FromQuery] CityQuery query, CancellationToken ct)
        => Ok(await cities.GetPagedAsync(query, ct));

    /// Gets the cities of a country with pagination.
    /// Sortable fields: name, population, country, createdAt
    [HttpGet("/api/countries/{countryId:int}/cities")]
    [ProducesResponseType<PagedResult<CityResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<CityResponse>>> GetCitiesByCountry(
        int countryId, [FromQuery] CountryCitiesQuery query, CancellationToken ct)
        => Ok(await cities.GetByCountryAsync(countryId, query, ct));

    [HttpGet("{id:int}", Name = nameof(GetCityById))]
    [ProducesResponseType<CityResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CityResponse>> GetCityById(int id, CancellationToken ct)
        => Ok(await cities.GetByIdAsync(id, ct));

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<CityResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CityResponse>> CreateCity(CreateCityRequest request, CancellationToken ct)
    {
        var created = await cities.CreateAsync(request, ct);
        return CreatedAtRoute(nameof(GetCityById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType<CityResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CityResponse>> UpdateCity(int id, UpdateCityRequest request, CancellationToken ct)
        => Ok(await cities.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCity(int id, CancellationToken ct)
    {
        await cities.DeleteAsync(id, ct);
        return NoContent();
    }
}
