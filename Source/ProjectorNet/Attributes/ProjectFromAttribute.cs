namespace ProjectorNet.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class ProjectFromAttribute : Attribute
{
    public ProjectFromAttribute(Type type)
    {
        Type = type;
    }

    public Type Type { get; }

    public Type? ContextType { get; set; }
}
