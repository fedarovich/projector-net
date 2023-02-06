#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ProjectorNet.Generator;

public static class DictionaryExtensions
{
    public static bool TryGetString(
        this IReadOnlyDictionary<string, TypedConstant> attributes, 
        string name, 
        [NotNullWhen(true)] out string? value)
    {
        if (attributes.TryGetValue(name, out var typedConstant) &&
            typedConstant is { Kind: TypedConstantKind.Primitive, IsNull: false, Value: string strValue })
        {
            value = strValue;
            return true;
        }

        value = default;
        return false;
    }

    public static bool TryGetBoolean(
        this IReadOnlyDictionary<string, TypedConstant> attributes,
        string name,
        out bool value)
    {
        if (attributes.TryGetValue(name, out var typedConstant) &&
            typedConstant is { Kind: TypedConstantKind.Primitive, IsNull: false, Value: bool boolValue })
        {
            value = boolValue;
            return true;
        }

        value = default;
        return false;
    }

    public static bool TryGetInt32(
        this IReadOnlyDictionary<string, TypedConstant> attributes,
        string name,
        out int value)
    {
        if (attributes.TryGetValue(name, out var typedConstant) &&
            typedConstant is { Kind: TypedConstantKind.Primitive or TypedConstantKind.Enum, IsNull: false, Value: int intValue })
        {
            value = intValue;
            return true;
        }

        value = default;
        return false;
    }

    public static bool TryGetType(
        this IReadOnlyDictionary<string, TypedConstant> attributes,
        string name,
        [NotNullWhen(true)] out INamedTypeSymbol? value)
    {
        if (attributes.TryGetValue(name, out var typedConstant) &&
            typedConstant is { Kind: TypedConstantKind.Type, IsNull: false, Value: INamedTypeSymbol typeSymbol })
        {
            value = typeSymbol;
            return true;
        }

        value = default;
        return false;
    }
}
