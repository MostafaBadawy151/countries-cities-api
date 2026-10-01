using BridgeTask.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BridgeTask.Api.Data.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public const int NameMaxLength = 100;

    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(NameMaxLength)
            .UseCollation("NOCASE");

        // A city name must be unique within its country.
        builder.HasIndex(c => new { c.CountryId, c.Name }).IsUnique();
    }
}
