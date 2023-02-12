#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace ProjectorNet.Generator.Model;

public class ProjectionPropertyProvider
{
    private static readonly IReadOnlyDictionary<string, TypedConstant> EmptyDictionary = new Dictionary<string, TypedConstant>();

    private readonly INamedTypeSymbol _projectionPropertyAttributeSymbol;
    private readonly INamedTypeSymbol _projectionParameterAttributeSymbol;
    private readonly INamedTypeSymbol _collectionProjectionAttributeSymbol;
    
    public ProjectionPropertyProvider(Compilation compilation)
    {
        _projectionPropertyAttributeSymbol = compilation.GetTypeByMetadataName("ProjectorNet.Attributes.ProjectionPropertyAttribute")!;
        _projectionParameterAttributeSymbol = compilation.GetTypeByMetadataName("ProjectorNet.Attributes.ProjectionParameterAttribute")!;
        _collectionProjectionAttributeSymbol = compilation.GetTypeByMetadataName("ProjectorNet.Attributes.CollectionProjectionAttribute")!;
    }

    public ProjectionProperty FromProperty(IPropertySymbol propertySymbol)
    {
        var attributes = propertySymbol.GetAttributes();
        var projectionAttribute = GetAttribute(attributes, _projectionPropertyAttributeSymbol);
        var collectionProjectionAttribute = GetAttribute(attributes, _collectionProjectionAttributeSymbol);

        GetProjectionOptions(projectionAttribute, out var sourceName, out var ignore, out var useDefaultValue,
            out var conversionMethod, out var expression, out var defaultValue);
        GetCollectionOptions(collectionProjectionAttribute, out var itemExpression, out var collectionType);

        return new ProjectionProperty
        {
            Name = propertySymbol.Name,
            Type = propertySymbol.Type,
            SourceName = string.IsNullOrWhiteSpace(sourceName) ? propertySymbol.Name : sourceName!,
            Ignore = ignore,
            UseDefaultValue = useDefaultValue,
            ConversionMethod = conversionMethod,
            Expression = expression,
            DefaultValue = defaultValue,
            ItemExpression = itemExpression,
            CollectionType = collectionType
        };
    }

    public ProjectionProperty FromParameter(IParameterSymbol propertySymbol)
    {
        var attributes = propertySymbol.GetAttributes();
        var projectionAttribute = GetAttribute(attributes, _projectionParameterAttributeSymbol);
        var collectionProjectionAttribute = GetAttribute(attributes, _collectionProjectionAttributeSymbol);

        GetProjectionOptions(projectionAttribute, out var sourceName, out var ignore, out var useDefaultValue,
            out var conversionMethod, out var expression, out var defaultValue);
        GetCollectionOptions(collectionProjectionAttribute, out var itemExpression, out var collectionType);

        return new ProjectionProperty
        {
            Name = propertySymbol.Name,
            Type = propertySymbol.Type,
            SourceName = string.IsNullOrWhiteSpace(sourceName) ? propertySymbol.Name.Pascalize() : sourceName!,
            Ignore = ignore,
            UseDefaultValue = useDefaultValue,
            ConversionMethod = conversionMethod,
            Expression = expression,
            DefaultValue = defaultValue,
            ItemExpression = itemExpression,
            CollectionType = collectionType
        };
    }

    private void GetProjectionOptions(AttributeData? projectionAttribute,
        out string? sourceName, out bool ignore, out bool useDefaultValue,
        out string? conversionMethod, out string? expression, out TypedConstant defaultValue)
    {
        var options = projectionAttribute?.NamedArguments.ToDictionary(x => x.Key, x => x.Value) ?? EmptyDictionary;
        options.TryGetString("SourceName", out sourceName);
        options.TryGetBoolean("Ignore", out ignore);
        options.TryGetBoolean("UseDefaultValue", out useDefaultValue);
        options.TryGetString("ConversionMethod", out conversionMethod);
        options.TryGetString("Expression", out expression);
        options.TryGetValue("DefaultValue", out defaultValue);
    }

    private void GetCollectionOptions(AttributeData? collectionProjectionAttribute,
        out string? itemExpression, out int collectionType)
    {
        var options = collectionProjectionAttribute?.NamedArguments.ToDictionary(x => x.Key, x => x.Value) ?? EmptyDictionary;
        options.TryGetString("ItemExpression", out itemExpression);
        options.TryGetInt32("CollectionType", out collectionType);
    }

    private static AttributeData? GetAttribute(ImmutableArray<AttributeData> attributes, INamedTypeSymbol attributeType) =>
        attributes.FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType));
}
