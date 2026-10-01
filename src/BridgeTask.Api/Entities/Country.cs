namespace BridgeTask.Api.Entities;

public class Country : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// ISO 3166-1 alpha-2 code, e.g. "EG".
    public string Code { get; set; } = string.Empty;

    public ICollection<City> Cities { get; set; } = new List<City>();
}
