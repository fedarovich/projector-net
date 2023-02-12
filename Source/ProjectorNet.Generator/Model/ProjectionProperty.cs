#nullable enable
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ProjectorNet.Generator.Model;

public sealed record ProjectionProperty
{
    public required string Name { get; init; }

    public required ITypeSymbol Type { get; init; }

    public required bool Ignore { get; init; }

    public required bool UseDefaultValue { get; init; }

    public required string SourceName { get; init; }

    public required string? ConversionMethod { get; init; }

    public required string? Expression { get; init; }

    public required TypedConstant DefaultValue { get; init; }

    public required string? ItemExpression { get; init; }

    public required int CollectionType { get; init; }
}
