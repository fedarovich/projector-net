namespace ProjectorNet.Generator.Model.Mappings;

public record ProjectionPropertyMapping(
    string ProjectionName, 
    TypeName ProjectionType,
    string SourcePath,
    TypeName SourceType
) : PropertyMapping(ProjectionName);
