using BridgeTask.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BridgeTask.Api.Data.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public const int NameMaxLength = 100;
    public const int CodeLength = 2;

    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(NameMaxLength)
            .UseCollation("NOCASE");

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(CodeLength)
            .IsFixedLength()
            .UseCollation("NOCASE");

        builder.HasIndex(c => c.Name).IsUnique();
        builder.HasIndex(c => c.Code).IsUnique();

        builder.HasMany(c => c.Cities)
            .WithOne(c => c.Country)
            .HasForeignKey(c => c.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
