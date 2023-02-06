#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ProjectorNet.Generator.Model;
using ProjectorNet.Generator.Model.Mappings;
using TypeInfo = ProjectorNet.Generator.Model.TypeInfo;
using TypeKind = ProjectorNet.Generator.Model.TypeKind;

namespace ProjectorNet.Generator;

[Generator(LanguageNames.CSharp)]
public class ProjectionGenerator : IIncrementalGenerator
{
    private static readonly ImmutableDictionary<SpecialType, PrimitiveType> PrimitiveTypes =
        new PrimitiveType[]
        {
            new (SpecialType.System_Boolean, "bool", "false", false),
            new (SpecialType.System_Byte, "byte", "0"),
            new (SpecialType.System_SByte, "sbyte", "0"),
            new (SpecialType.System_Int16, "short", "0"),
            new (SpecialType.System_UInt16, "ushort", "0"),
            new (SpecialType.System_Int32, "int", "0"),
            new (SpecialType.System_UInt32, "uint", "0U"),
            new (SpecialType.System_Int64, "long", "0L"),
            new (SpecialType.System_UInt64, "ulong", "0UL"),
            new (SpecialType.System_Decimal, "decimal", "0m"),
            new (SpecialType.System_Single, "float", "0f"),
            new (SpecialType.System_Double, "decimal", "0d"),
        }.ToImmutableDictionary(x => x.SpecialType);

    private const string EmptyStringLiteral = "\"\"";
    private const string DefaultLiteral = "default";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var nullableContextOptionsProvider = context.CompilationProvider.Select((c, _) => c.Options.NullableContextOptions);

        var projectionProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "ProjectorNet.Attributes.ProjectFromAttribute",
            (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax,
            (ctx, _) =>
            {
                if (ctx.TargetSymbol is not INamedTypeSymbol projectionTypeSymbol)
                    return null;

                var projectionType = TypeInfo.FromSymbol(projectionTypeSymbol);

                var compilation = ctx.SemanticModel.Compilation;
                var projectionPropertyAttributeSymbol = compilation.GetTypeByMetadataName("ProjectorNet.Attributes.ProjectionPropertyAttribute")!;
                var projectionConstructorAttributeSymbol = compilation.GetTypeByMetadataName("ProjectorNet.Attributes.ProjectionConstructorAttribute")!;

                var projectionAttribute = ctx.Attributes.FirstOrDefault();
                if (projectionAttribute is not { ConstructorArguments: [{ Value: INamedTypeSymbol sourceTypeSymbol }, ..] })
                    return null;

                var sourceTypeName = TypeName.FromSymbol(sourceTypeSymbol);
                var sourceProperties = sourceTypeSymbol.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(p => !p.IsIndexer && !p.IsWriteOnly)
                    .ToDictionary(p => p.Name);

                var projectionConstructor = GetProjectionConstructor(projectionTypeSymbol, projectionConstructorAttributeSymbol, ctx.SemanticModel);

                var propertyMappings = projectionTypeSymbol.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(p => !p.IsIndexer && !p.IsReadOnly)
                    .Select(p => GetPropertyMapping(p, sourceProperties, projectionPropertyAttributeSymbol))
                    .ToImmutableArray();

                var namedArguments = projectionAttribute.NamedArguments.ToDictionary(x => x.Key, x => x.Value);
                var contextTypeName = namedArguments.TryGetValue("ContextType", out var value) && value.Value is INamedTypeSymbol contextTypeSymbol
                    ? TypeName.FromSymbol(contextTypeSymbol)
                    : (TypeName?) null;

                return new Projection
                {
                    ProjectionType = projectionType,
                    SourceTypeName = sourceTypeName,
                    ContextTypeName = contextTypeName,
                    PropertyMappings = propertyMappings
                };
            }
        ).Where(p => p != null);

        var projectionsInNamespaceProvider = projectionProvider.Collect()
            .SelectMany((projections, _) => projections.GroupBy(p => p!.ProjectionType.Name.Namespace));

        context.RegisterSourceOutput(
            projectionProvider.Combine(nullableContextOptionsProvider),
            (ctx, arg) =>
            {
                var (projection, nullableContextOptions) = arg;
                var typeName = projection!.ProjectionType.Name;
                var sourceType = projection.SourceTypeName.FullyQualifiedName;
                var contextType = projection.ContextTypeName?.FullyQualifiedName ?? GetObjectType(nullableContextOptions);
                
                ctx.AddSource($"{typeName.Namespace}.{typeName.Name}.g.cs",
                    builder =>
                    {
                        if (nullableContextOptions.AnnotationsEnabled())
                            builder.AppendLine("#nullable enable");

                        using var ns = builder.BeginNamespace(typeName.Namespace);
                        builder.AppendLine("using global::System.Linq;");
                        builder.AppendLine();
                        using var typeDeclaration = builder.BeginTypeDeclaration(projection.ProjectionType,
                            $"global::ProjectorNet.IProjection<{typeName.Name}, {sourceType}, {contextType}>");
                        builder.AppendLine(
                            $"public static global::System.Linq.Expressions.Expression<Func<{sourceType}, {typeName.Name}>> GetProjectionExpression({contextType} context) =>");
                        using var indent = builder.Indent();
                        builder.AppendLine($"source => new {typeName.Name}");
                        AppendPropertyMappings(projection.PropertyMappings);

                        void AppendPropertyMappings(in PropertyMappingCollection propertyMappings)
                        {
                            using var mappingBlock = builder.BeginBlock(";");
                            foreach (var propertyMapping in propertyMappings)
                            {
                                if (propertyMapping is IgnoredPropertyMapping)
                                    continue;

                                builder.Append($"{propertyMapping.ProjectionName} = ");
                                AppendPropertyMapping(propertyMapping);
                                builder.AppendLine(",");
                            }
                        }

                        void AppendPropertyMapping(PropertyMapping propertyMapping)
                        {
                            switch (propertyMapping)
                            {
                                case ExpressionPropertyMapping epm:
                                    builder.Append($"{epm.Expression}");
                                    break;
                                case CollectionPropertyMapping cpm:
                                    builder.AppendLine($"{cpm.SourceName}.Select(source => ");
                                    using (builder.Indent())
                                    {
                                        AppendPropertyMapping(cpm.ItemMapping);
                                        builder.AppendLine();
                                    }
                                    builder.Append(")");
                                    builder.Append(cpm.CollectionTransform switch
                                    {
                                        CollectionTransform.None => string.Empty,
                                        CollectionTransform.ToArray => ".ToArray()",
                                        CollectionTransform.ToList => ".ToList()",
                                        CollectionTransform.ToHashSet => ".ToHashSet()",
                                        _ => throw new ArgumentOutOfRangeException()
                                    });
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(propertyMapping));
                            }
                        }
                    });
            });

        context.RegisterSourceOutput(
            projectionsInNamespaceProvider.Combine(nullableContextOptionsProvider),
            (ctx, arg) =>
            {
                var (projectionGroup, nullableContextOptions) = arg;
                var extType = new TypeName(
                    "GeneratedProjectorExtensions",
                    projectionGroup.Key,
                    $"global::{projectionGroup.Key}.ProjectorExtensions");

                ctx.AddSource($"{projectionGroup.Key}.ProjectorExtensions.g.cs",
                    builder =>
                    {
                        if (nullableContextOptions.AnnotationsEnabled())
                            builder.AppendLine("#nullable enable");

                        using var ns = builder.BeginNamespace(projectionGroup.Key);
                        builder.AppendLine("using global::System.Linq;");
                        builder.AppendLine();
                        using var typeDeclaration = builder.BeginTypeDeclaration("public static", new TypeInfo(extType, TypeKind.Class));

                        foreach (var projection in projectionGroup)
                        {
                            builder.AppendLine(
                                $"public static global::System.Linq.IQueryable<{projection!.ProjectionType.Name.FullyQualifiedName}> "
                                + $"To{projection.ProjectionType.Name.Name}(this global::ProjectorNet.IProjector<{projection.SourceTypeName.FullyQualifiedName}, "
                                + $"{projection.ContextTypeName?.FullyQualifiedName ?? GetObjectType(nullableContextOptions)}> projector) =>");
                            builder.AppendLine(
                                $"    projector.Query.Select({projection.ProjectionType.Name.FullyQualifiedName}.GetProjectionExpression(projector.Context));");
                            builder.AppendLine();
                        }
                    });
            });
    }

    private static PropertyMapping GetPropertyMapping(IPropertySymbol propertySymbol,
        Dictionary<string, IPropertySymbol> sourceProperties,
        INamedTypeSymbol projectionPropertyAttributeSymbol)
    {
        var sourceName = propertySymbol.Name;
        string? conversionMethod = null;
        TypedConstant defaultValue = default;
        int customCollectionType = 0;

        var projectionAttribute = propertySymbol
            .GetAttributes()
            .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, projectionPropertyAttributeSymbol));

        if (projectionAttribute != null)
        {
            var options = projectionAttribute.NamedArguments.ToDictionary(
                x => x.Key, 
                x => x.Value);

            if (options.TryGetBoolean("Ignore", out var ignore) && ignore)
                return new IgnoredPropertyMapping(propertySymbol.Name);

            if (options.TryGetString("Expression", out var expression) && !string.IsNullOrWhiteSpace(expression))
                return new ExpressionPropertyMapping(propertySymbol.Name, expression);

            if (options.TryGetString("SourceName", out var sn) && !string.IsNullOrWhiteSpace(sn))
                sourceName = sn;

            options.TryGetString("ConversionMethod", out conversionMethod);

            options.TryGetValue("DefaultValue", out defaultValue);

            options.TryGetInt32("CollectionType", out customCollectionType);
        }

        if (!sourceProperties.TryGetValue(sourceName, out var sourcePropertySymbol))
            return new ExpressionPropertyMapping(propertySymbol.Name, "source." +  sourceName); // Return direct mapping to get compile time error.

        if (string.IsNullOrWhiteSpace(conversionMethod) && SymbolEqualityComparer.IncludeNullability.Equals(propertySymbol.Type, sourcePropertySymbol.Type))
            return new ExpressionPropertyMapping(propertySymbol.Name, "source." + sourceName);

        // TODO: Handle collections
        var collectionType = GetCollectionType(propertySymbol.Type, out var collectionElementType);
        var sourceCollectionType = GetCollectionType(sourcePropertySymbol.Type, out var sourceCollectionElementType);

        if (collectionType != CollectionType.None && sourceCollectionType != CollectionType.None)
        {
            var itemConversionExpression = GetConversionExpression(
                collectionElementType!, sourceCollectionElementType!, "source", conversionMethod, defaultValue);
            PropertyMapping itemPropertyMapping = itemConversionExpression != null
                ? new ExpressionPropertyMapping("source", itemConversionExpression)
                : new ProjectionPropertyMapping("source", TypeName.FromSymbol(collectionElementType), TypeName.FromSymbol(sourceCollectionElementType));

            var collectionTypeAfterTransform = (CollectionType) Math.Max((int) collectionType, customCollectionType);

            var transform = collectionTypeAfterTransform switch
            {
                CollectionType.None => CollectionTransform.None,
                CollectionType.Enumerable => CollectionTransform.None,
                CollectionType.Collection => CollectionTransform.ToHashSet,
                CollectionType.List => CollectionTransform.ToList,
                CollectionType.Array => CollectionTransform.ToArray,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            return new CollectionPropertyMapping(propertySymbol.Name, "source." + sourceName, transform, itemPropertyMapping);
        }

        var conversionExpression = GetConversionExpression(
            propertySymbol.Type, sourcePropertySymbol.Type, "source." + sourceName, conversionMethod, defaultValue);
        return conversionExpression != null
            ? new ExpressionPropertyMapping(propertySymbol.Name, conversionExpression)
            : new ProjectionPropertyMapping(propertySymbol.Name, TypeName.FromSymbol(propertySymbol.Type), TypeName.FromSymbol(sourcePropertySymbol.Type));
    }

    private static string GetObjectType(NullableContextOptions nullableContextOptions) =>
        nullableContextOptions.AnnotationsEnabled() ? "object?" : "object";

    private static string? GetConversionExpression(ITypeSymbol targetType, ITypeSymbol sourceType, string sourceName, string? conversionMethod, in TypedConstant defaultValue)
    {
        if (!string.IsNullOrWhiteSpace(conversionMethod))
            return $"context.{conversionMethod}({sourceName})";

        bool isTargetNullable = IsNullable(targetType, out var baseTargetType);
        bool isSourceNullable = IsNullable(sourceType, out var baseSourceType);

        if (baseTargetType.SpecialType == SpecialType.System_String)
        {
            if (baseSourceType.SpecialType == SpecialType.System_String)
            {
                return isTargetNullable || !isSourceNullable ? sourceName : $"({sourceName} ?? {GetDefaultValueLiteral(defaultValue, EmptyStringLiteral)})";
            }

            return isSourceNullable
                ? $"({sourceName} != null ? {sourceName}.ToString() : {(isTargetNullable ? "null" : GetDefaultValueLiteral(defaultValue, EmptyStringLiteral))})"
                : $"{sourceName}.ToString()";
        }

        if (SymbolEqualityComparer.Default.Equals(baseTargetType, baseSourceType))
        {
            return (!isTargetNullable && isSourceNullable) ? $"({sourceName} ?? {GetDefaultValueLiteral(defaultValue, DefaultLiteral)})" : sourceName;
        }

        if (IsNumericOrEnum(baseTargetType, out var targetTypeAlias) && IsNumericOrEnum(baseSourceType, out _))
        {
            return (isTargetNullable, isSourceNullable) switch
            {
                (_, false) => $"({targetTypeAlias}) {sourceName}",
                (false, true) => $"({targetTypeAlias}) {sourceName}.GetValueOrDefault({GetDefaultValueLiteral(defaultValue)})",
                (true, true) => $"({targetTypeAlias}?) {sourceName}"
            };
        }

        if (PrimitiveTypes.TryGetValue(baseTargetType.SpecialType, out var primitiveType))
        {
            return (isTargetNullable, isSourceNullable) switch
            {
                (_, false) => $"{primitiveType.ConversionMethod}({sourceName})",
                (false, true) => $"({sourceName} != null ? {primitiveType.ConversionMethod}({sourceName}) : {GetDefaultValueLiteral(defaultValue, primitiveType.DefaultValue)})",
                (true, true) => $"({sourceName} != null ? {primitiveType.ConversionMethod}({sourceName}) : ({primitiveType.Name}?) null)"
            };
        }

        return null;
    }

    private static bool IsNullable(ITypeSymbol type, out ITypeSymbol baseTypeSymbol)
    {
        baseTypeSymbol = type;
        bool nullable = false;

        if (type.IsReferenceType)
        {
            if (type.NullableAnnotation != NullableAnnotation.NotAnnotated)
            {
                baseTypeSymbol = type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
                nullable = true;
            }
        }
        else
        {
            if (baseTypeSymbol is INamedTypeSymbol
                {
                    IsGenericType: true,
                    ConstructedFrom.SpecialType: SpecialType.System_Nullable_T,
                    TypeArguments: [var @base]
                })
            {
                baseTypeSymbol = @base;
                nullable = true;
            }
        }

        return nullable;
    }

    private static bool IsNumericOrEnum(ITypeSymbol typeSymbol, [NotNullWhen(true)] out string? typeAlias)
    {
        if (PrimitiveTypes.TryGetValue(typeSymbol.SpecialType, out var primitiveType) && primitiveType.IsNumeric)
        {
            typeAlias = primitiveType.Name;
            return true;
        }

        if (typeSymbol.BaseType is { SpecialType: SpecialType.System_Enum })
        {
            typeAlias = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return true;
        }

        typeAlias = null;
        return false;
    }

    private static string GetDefaultValueLiteral(in TypedConstant typedConstant, string alternateValue = "")
    {
        var literal = typedConstant.ToCSharpString();
        return typedConstant.Kind switch
        {
            TypedConstantKind.Error => alternateValue,
            TypedConstantKind.Enum when char.IsNumber(literal[0]) || literal[0] == '-' => 
                $"({typedConstant.Type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}) ({literal})",
            TypedConstantKind.Enum => "global::" + literal,
            TypedConstantKind.Type => $"typeof({(typedConstant.Value as ITypeSymbol)!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})",
            _ => typedConstant.ToCSharpString()
        };
    }

    private static IMethodSymbol? GetProjectionConstructor(INamedTypeSymbol projectionType,
        INamedTypeSymbol projectionConstructorSymbol, SemanticModel semanticModel)
    {
        var constructors = projectionType.InstanceConstructors;
        if (constructors.Length == 1)
            return constructors[0];

        var projectionConstructorCandidates = constructors
            .Where(c => c.GetAttributes().Any(a =>
                SymbolEqualityComparer.Default.Equals(a.AttributeClass, projectionConstructorSymbol)))
            .ToImmutableArray();
        
        if (projectionConstructorCandidates.Length == 1)
            return projectionConstructorCandidates[0];

        var defaultConstructor = constructors.FirstOrDefault(c => c.Parameters.Length == 0);
        if (defaultConstructor != null)
            return defaultConstructor;

        if (!projectionType.IsRecord)
            return null;

        var pls = projectionType.DeclaringSyntaxReferences
            .Select(r => r.GetSyntax())
            .FirstOrDefault(r => r.ChildNodes().OfType<ParameterListSyntax>().Any());
        if (pls == null)
            return null;

        var primaryConstructorCandidates =
            from c in constructors
            where c.DeclaringSyntaxReferences.Length == 1
            let sr = c.DeclaringSyntaxReferences[0]
            where sr.SyntaxTree == pls.SyntaxTree && pls.Span.Contains(sr.Span)
            orderby sr.Span.Start
            select c;

        return primaryConstructorCandidates.FirstOrDefault();
    }

    private static CollectionType GetCollectionType(ITypeSymbol typeSymbol, out ITypeSymbol? collectionElement)
    {
        collectionElement = null;

        if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
        {
            collectionElement = arrayTypeSymbol.ElementType;
            return CollectionType.Array;
        }

        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            return CollectionType.None;

        if (namedTypeSymbol.SpecialType == SpecialType.System_String)
            return CollectionType.None;

        INamedTypeSymbol? @interface = null;

        if (IsList(namedTypeSymbol.ConstructedFrom) || 
            (@interface = namedTypeSymbol.Interfaces.FirstOrDefault(i => IsList(i.ConstructedFrom))) != null)
        {
            collectionElement = @interface?.TypeArguments[0] ?? namedTypeSymbol.TypeArguments[0];
            return CollectionType.List;
        }

        if (IsCollection(namedTypeSymbol.ConstructedFrom) ||
            (@interface = namedTypeSymbol.Interfaces.FirstOrDefault(i => IsCollection(i.ConstructedFrom))) != null)
        {
            collectionElement = @interface?.TypeArguments[0] ?? namedTypeSymbol.TypeArguments[0];
            return CollectionType.Collection;
        }

        if (IsEnumerable(namedTypeSymbol.ConstructedFrom) ||
            (@interface = namedTypeSymbol.Interfaces.FirstOrDefault(i => IsEnumerable(i.ConstructedFrom))) != null)
        {
            collectionElement = @interface?.TypeArguments[0] ?? namedTypeSymbol.TypeArguments[0];
            return CollectionType.Enumerable;
        }

        return CollectionType.None;

        bool IsList(INamedTypeSymbol symbol) => 
            symbol.SpecialType is SpecialType.System_Collections_Generic_IList_T or SpecialType.System_Collections_Generic_IReadOnlyList_T;

        bool IsCollection(INamedTypeSymbol symbol) =>
            symbol.SpecialType is SpecialType.System_Collections_Generic_ICollection_T or SpecialType.System_Collections_Generic_IReadOnlyCollection_T;

        bool IsEnumerable(INamedTypeSymbol symbol) =>
            symbol.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T;
    }

    private record PrimitiveType(SpecialType SpecialType, string Name, string DefaultValue, bool IsNumeric = true)
    {
        public string ConversionMethod { get; } = "global::System.Convert.To" + SpecialType.ToString().Substring(7);
    }
}
