namespace ProjectorNet.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class CollectionProjectionAttribute : Attribute
{
    public CollectionType CollectionType { get; set; }

    public string? ItemExpression { get; set; }
}
