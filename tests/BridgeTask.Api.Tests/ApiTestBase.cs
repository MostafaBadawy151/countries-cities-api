using System.Net.Http.Json;
using BridgeTask.Api.Dtos.Cities;
using BridgeTask.Api.Dtos.Countries;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace BridgeTask.Api.Tests;

/// Spins up the API against a fresh, unseeded SQLite database for every test.
public abstract class ApiTestBase : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"bridgetask-test-{Guid.NewGuid():N}.db");
    private readonly WebApplicationFactory<Program> _factory;

    protected HttpClient Client { get; }

    protected ApiTestBase()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
            builder.UseSetting("Database:Seed", "false");
        });
        Client = _factory.CreateClient();
    }

    protected async Task<CountryResponse> CreateCountryAsync(string name, string code)
    {
        var response = await Client.PostAsJsonAsync("/api/countries", new CreateCountryRequest(name, code));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CountryResponse>())!;
    }

    protected async Task<CityResponse> CreateCityAsync(string name, int countryId, long? population = null)
    {
        var response = await Client.PostAsJsonAsync("/api/cities", new CreateCityRequest(name, countryId, population));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CityResponse>())!;
    }

    public void Dispose()
    {
        Client.Dispose();
        _factory.Dispose();
        SqliteConnection.ClearAllPools();
        File.Delete(_dbPath);
        GC.SuppressFinalize(this);
    }
}
