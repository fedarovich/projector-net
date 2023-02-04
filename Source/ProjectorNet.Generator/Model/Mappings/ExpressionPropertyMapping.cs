namespace ProjectorNet.Generator.Model.Mappings;

public record ExpressionPropertyMapping(string ProjectionName, string Expression) : PropertyMapping(ProjectionName);
