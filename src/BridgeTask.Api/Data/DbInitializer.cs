using BridgeTask.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BridgeTask.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, bool seed)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync();

        if (!seed || await db.Countries.AnyAsync())
            return;

        db.Countries.AddRange(
            new Country
            {
                Name = "Egypt", Code = "EG",
                Cities =
                [
                    new City { Name = "Cairo", Population = 10_100_000 },
                    new City { Name = "Alexandria", Population = 5_200_000 },
                    new City { Name = "Giza", Population = 4_400_000 }
                ]
            },
            new Country
            {
                Name = "Saudi Arabia", Code = "SA",
                Cities =
                [
                    new City { Name = "Riyadh", Population = 7_600_000 },
                    new City { Name = "Jeddah", Population = 4_700_000 }
                ]
            },
            new Country
            {
                Name = "United Arab Emirates", Code = "AE",
                Cities =
                [
                    new City { Name = "Dubai", Population = 3_600_000 },
                    new City { Name = "Abu Dhabi", Population = 1_500_000 }
                ]
            });

        await db.SaveChangesAsync();
    }
}
