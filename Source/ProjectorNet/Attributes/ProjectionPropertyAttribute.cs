namespace ProjectorNet.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ProjectionPropertyAttribute : Attribute
{
    public string? SourceName { get; set; }

    public string? ConversionMethod { get; set; }

    public string? Expression { get; set; }

    public bool Ignore { get; set; }

    public object? DefaultValue { get; set; }
}
