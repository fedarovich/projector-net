namespace ProjectorNet.Generator.Model.Mappings;

public record CollectionPropertyMapping(
    string ProjectionName,
    string SourceName,
    TypeName SourceItemType,
    CollectionTransform CollectionTransform,
    PropertyMapping ItemMapping
) : PropertyMapping(ProjectionName);
