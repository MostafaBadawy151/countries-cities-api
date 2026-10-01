using BridgeTask.Api.Common.Pagination;
using BridgeTask.Api.Dtos.Countries;
using BridgeTask.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BridgeTask.Api.Controllers;

[ApiController]
[Route("api/countries")]
[Produces("application/json")]
public sealed class CountriesController(ICountryService countries) : ControllerBase
{
    /// Gets countries with pagination, filtering and sorting.
    /// Sortable fields: name, code, createdAt
    [HttpGet]
    [ProducesResponseType<PagedResult<CountryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CountryResponse>>> GetCountries(
        [FromQuery] CountryQuery query, CancellationToken ct)
        => Ok(await countries.GetPagedAsync(query, ct));

    /// Gets a country by id.
    [HttpGet("{id:int}", Name = nameof(GetCountryById))]
    [ProducesResponseType<CountryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CountryResponse>> GetCountryById(int id, CancellationToken ct)
        => Ok(await countries.GetByIdAsync(id, ct));

    /// Creates a country.
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<CountryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CountryResponse>> CreateCountry(CreateCountryRequest request, CancellationToken ct)
    {
        var created = await countries.CreateAsync(request, ct);
        return CreatedAtRoute(nameof(GetCountryById), new { id = created.Id }, created);
    }

    /// Updates a country.
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType<CountryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CountryResponse>> UpdateCountry(
        int id, UpdateCountryRequest request, CancellationToken ct)
        => Ok(await countries.UpdateAsync(id, request, ct));

    /// Deletes a country. Fails with 409 if the country still has cities.
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCountry(int id, CancellationToken ct)
    {
        await countries.DeleteAsync(id, ct);
        return NoContent();
    }
}
