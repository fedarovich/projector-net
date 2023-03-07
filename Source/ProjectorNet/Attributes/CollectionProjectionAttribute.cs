namespace ProjectorNet.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class CollectionProjectionAttribute : Attribute
{
    public CollectionType CollectionType { get; set; }

    public string? ItemExpression { get; set; }

    public Type? ItemTypeFilter { get; set; }

    public string? FilterExpression { get; set; }
}
