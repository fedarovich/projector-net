namespace ProjectorNet.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class ProjectionPropertyAttribute : Attribute
{
    public string? SourceName { get; set; }

    public string? ConversionMethod { get; set; }

    public string? Expression { get; set; }

    public bool Ignore { get; set; }

    public object? DefaultValue { get; set; }

    public CollectionType CollectionType { get; set; }
}
