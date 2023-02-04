namespace ProjectorNet.Samples.Entities;

public class Person
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public ICollection<string> Tags { get; set; } = null!;

    public string IntAsString { get; set; } = null!;

    public int A { get; set; }

    public int? B { get; set; }

    public string? C { get; set; }
}
