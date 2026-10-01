using System.Net;
using System.Net.Http.Json;
using BridgeTask.Api.Common.Pagination;
using BridgeTask.Api.Dtos.Countries;
using Microsoft.AspNetCore.Mvc;

namespace BridgeTask.Api.Tests;

public class CountriesApiTests : ApiTestBase
{
    [Fact]
    public async Task Create_ReturnsCreated_WithLocationAndNormalizedCode()
    {
        var response = await Client.PostAsJsonAsync("/api/countries", new CreateCountryRequest("  Egypt ", "eg"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var country = await response.Content.ReadFromJsonAsync<CountryResponse>();
        Assert.Equal("Egypt", country!.Name);
        Assert.Equal("EG", country.Code);
        Assert.Equal(DateTimeKind.Utc, country.CreatedAtUtc.Kind);
        Assert.EndsWith($"/api/countries/{country.Id}", response.Headers.Location!.ToString());
    }

    [Theory]
    [InlineData("", "EG")]
    [InlineData("Egypt", "EGY")]
    [InlineData("Egypt", "1A")]
    public async Task Create_InvalidInput_ReturnsValidationProblem(string name, string code)
    {
        var response = await Client.PostAsJsonAsync("/api/countries", new CreateCountryRequest(name, code));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotEmpty(problem!.Errors);
    }

    [Fact]
    public async Task Create_DuplicateNameIgnoringCase_ReturnsConflict()
    {
        await CreateCountryAsync("Egypt", "EG");

        var response = await Client.PostAsJsonAsync("/api/countries", new CreateCountryRequest("EGYPT", "EZ"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Missing_ReturnsNotFoundProblem()
    {
        var response = await Client.GetAsync("/api/countries/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task GetPaged_FiltersSortsAndPaginates()
    {
        await CreateCountryAsync("Egypt", "EG");
        await CreateCountryAsync("Ecuador", "EC");
        await CreateCountryAsync("France", "FR");
        await CreateCountryAsync("Estonia", "EE");

        var page = await Client.GetFromJsonAsync<PagedResult<CountryResponse>>(
            "/api/countries?search=e&sortBy=name&sortDirection=desc&pageNumber=1&pageSize=2");

        Assert.Equal(4, page!.TotalCount); // "France" matches too
        Assert.Equal(2, page.TotalPages);
        Assert.Equal(["France", "Estonia"], page.Items.Select(c => c.Name));

        var byCode = await Client.GetFromJsonAsync<PagedResult<CountryResponse>>("/api/countries?code=ec");
        Assert.Equal("Ecuador", Assert.Single(byCode!.Items).Name);
    }

    [Theory]
    [InlineData("pageNumber=0")]
    [InlineData("pageSize=101")]
    [InlineData("sortBy=population")]
    public async Task GetPaged_InvalidQuery_ReturnsBadRequest(string queryString)
    {
        var response = await Client.GetAsync($"/api/countries?{queryString}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ChangesFields()
    {
        var country = await CreateCountryAsync("Egypt", "EG");

        var response = await Client.PutAsJsonAsync($"/api/countries/{country.Id}",
            new UpdateCountryRequest("Arab Republic of Egypt", "EG"));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<CountryResponse>();
        Assert.Equal("Arab Republic of Egypt", updated!.Name);
        Assert.Equal(DateTimeKind.Utc, updated.UpdatedAtUtc?.Kind);
    }

    [Fact]
    public async Task Update_ToAnotherCountrysCode_ReturnsConflict()
    {
        await CreateCountryAsync("Egypt", "EG");
        var france = await CreateCountryAsync("France", "FR");

        var response = await Client.PutAsJsonAsync($"/api/countries/{france.Id}", new UpdateCountryRequest("France", "EG"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithCities_ReturnsConflict_ThenSucceedsOnceEmpty()
    {
        var country = await CreateCountryAsync("Egypt", "EG");
        var city = await CreateCityAsync("Cairo", country.Id);

        Assert.Equal(HttpStatusCode.Conflict, (await Client.DeleteAsync($"/api/countries/{country.Id}")).StatusCode);

        await Client.DeleteAsync($"/api/cities/{city.Id}");
        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync($"/api/countries/{country.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync($"/api/countries/{country.Id}")).StatusCode);
    }
}
