#nullable enable
using System.Collections.Generic;
using System.Linq;
using ProjectorNet.Generator.Model.Mappings;

namespace ProjectorNet.Generator.Model;

public sealed record Projection
{
    public required TypeInfo ProjectionType { get; init; }
    
    public required TypeName SourceTypeName { get; init; }
    
    public TypeName? ContextTypeName { get; init; }

    public PropertyMappingCollection PropertyMappings { get; init; }

    public PropertyMappingCollection? ConstructorParameterMappings { get; init; }

    public IEnumerable<PropertyMapping> GetAllMappings()
    {
        return (ConstructorParameterMappings ?? Enumerable.Empty<PropertyMapping>()).Concat(PropertyMappings);
    }
}
