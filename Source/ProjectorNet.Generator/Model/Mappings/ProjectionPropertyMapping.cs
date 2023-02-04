namespace ProjectorNet.Generator.Model.Mappings;

public record ProjectionPropertyMapping(
    string ProjectionName, 
    TypeName ProjectionType, 
    TypeName SourceType
) : PropertyMapping(ProjectionName);
