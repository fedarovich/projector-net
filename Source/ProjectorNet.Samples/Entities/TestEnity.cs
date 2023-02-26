namespace ProjectorNet.Samples.Entities;

public class TestEntity : EntityBase
{
    public string? NullString { get; set; }

    public string NotNullString { get; set; } = null!;

    public SubEntity SubEntity { get; set; } = null!;

    public int? IntToLong { get; set; }

    public ICollection<SubEntity> SubEntities { get; set; } = null!;

    public ICollection<string?> Strings { get; set; } = null!;

    public ICollection<int> IntsToString { get; set; } = null!;
}
