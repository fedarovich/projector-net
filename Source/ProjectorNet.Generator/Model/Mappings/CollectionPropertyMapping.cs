namespace ProjectorNet.Generator.Model.Mappings;

public record CollectionPropertyMapping(
    string ProjectionName,
    string SourceName,
    CollectionTransform CollectionTransform,
    PropertyMapping ItemMapping
) : PropertyMapping(ProjectionName);
