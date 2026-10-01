namespace BridgeTask.Api.Entities;

public class City : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public long? Population { get; set; }

    public int CountryId { get; set; }
    public Country Country { get; set; } = null!;
}
