using System.Net;
using System.Net.Http.Json;
using BridgeTask.Api.Common.Pagination;
using BridgeTask.Api.Dtos.Cities;

namespace BridgeTask.Api.Tests;

public class CitiesApiTests : ApiTestBase
{
    [Fact]
    public async Task Create_ReturnsCreated_WithCountryName()
    {
        var egypt = await CreateCountryAsync("Egypt", "EG");

        var response = await Client.PostAsJsonAsync("/api/cities", new CreateCityRequest("Cairo", egypt.Id, 10_000_000));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var city = await response.Content.ReadFromJsonAsync<CityResponse>();
        Assert.Equal("Egypt", city!.CountryName);
        Assert.Equal(1, (await Client.GetFromJsonAsync<Dtos.Countries.CountryResponse>($"/api/countries/{egypt.Id}"))!.CitiesCount);
    }

    [Fact]
    public async Task Create_UnknownCountry_ReturnsNotFound()
    {
        var response = await Client.PostAsJsonAsync("/api/cities", new CreateCityRequest("Cairo", 42, null));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_NegativePopulation_ReturnsBadRequest()
    {
        var egypt = await CreateCountryAsync("Egypt", "EG");

        var response = await Client.PostAsJsonAsync("/api/cities", new CreateCityRequest("Cairo", egypt.Id, -1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateNameInSameCountry_ReturnsConflict_ButAllowedInOtherCountry()
    {
        var egypt = await CreateCountryAsync("Egypt", "EG");
        var usa = await CreateCountryAsync("United States", "US");
        await CreateCityAsync("Alexandria", egypt.Id);

        var duplicate = await Client.PostAsJsonAsync("/api/cities", new CreateCityRequest("alexandria", egypt.Id, null));
        var otherCountry = await Client.PostAsJsonAsync("/api/cities", new CreateCityRequest("Alexandria", usa.Id, null));

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.Created, otherCountry.StatusCode);
    }

    [Fact]
    public async Task GetPaged_FiltersByPopulationAndCountry()
    {
        var egypt = await CreateCountryAsync("Egypt", "EG");
        var france = await CreateCountryAsync("France", "FR");
        await CreateCityAsync("Cairo", egypt.Id, 10_000_000);
        await CreateCityAsync("Luxor", egypt.Id, 500_000);
        await CreateCityAsync("Paris", france.Id, 2_100_000);

        var big = await Client.GetFromJsonAsync<PagedResult<CityResponse>>(
            "/api/cities?minPopulation=1000000&sortBy=population&sortDirection=desc");
        Assert.Equal(["Cairo", "Paris"], big!.Items.Select(c => c.Name));

        var egyptian = await Client.GetFromJsonAsync<PagedResult<CityResponse>>($"/api/cities?countryId={egypt.Id}&search=LUX");
        Assert.Equal("Luxor", Assert.Single(egyptian!.Items).Name);
    }

    [Fact]
    public async Task GetPaged_MinGreaterThanMax_ReturnsBadRequest()
    {
        var response = await Client.GetAsync("/api/cities?minPopulation=10&maxPopulation=5");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByCountry_ReturnsOnlyThatCountrysCities_Paginated()
    {
        var egypt = await CreateCountryAsync("Egypt", "EG");
        var france = await CreateCountryAsync("France", "FR");
        await CreateCityAsync("Cairo", egypt.Id);
        await CreateCityAsync("Giza", egypt.Id);
        await CreateCityAsync("Aswan", egypt.Id);
        await CreateCityAsync("Paris", france.Id);

        var page2 = await Client.GetFromJsonAsync<PagedResult<CityResponse>>(
            $"/api/countries/{egypt.Id}/cities?pageNumber=2&pageSize=2");

        Assert.Equal(3, page2!.TotalCount);
        Assert.True(page2.HasPreviousPage);
        Assert.False(page2.HasNextPage);
        Assert.Equal("Giza", Assert.Single(page2.Items).Name);
    }

    [Fact]
    public async Task GetByCountry_UnknownCountry_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/countries/999/cities");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_MovesCityToAnotherCountry()
    {
        var egypt = await CreateCountryAsync("Egypt", "EG");
        var france = await CreateCountryAsync("France", "FR");
        var city = await CreateCityAsync("Paris", egypt.Id);

        var response = await Client.PutAsJsonAsync($"/api/cities/{city.Id}",
            new UpdateCityRequest("Paris", france.Id, 2_100_000));

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<CityResponse>();
        Assert.Equal(france.Id, updated!.CountryId);
        Assert.Equal(2_100_000, updated.Population);
    }

    [Fact]
    public async Task Delete_RemovesCity()
    {
        var egypt = await CreateCountryAsync("Egypt", "EG");
        var city = await CreateCityAsync("Cairo", egypt.Id);

        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync($"/api/cities/{city.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync($"/api/cities/{city.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.DeleteAsync($"/api/cities/{city.Id}")).StatusCode);
    }
}
