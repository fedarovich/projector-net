using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ProjectorNet.Generator;

public static class SymbolExtensions
{
    public static string GetFullyQualifiedName(this ISymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    public static IEnumerable<IPropertySymbol> GetAllProperties(this ITypeSymbol typeSymbol)
    {
        var result = Enumerable.Empty<IPropertySymbol>();
        while (typeSymbol != null && typeSymbol.SpecialType != SpecialType.System_Object)
        {
            result = result.Concat(typeSymbol.GetMembers().OfType<IPropertySymbol>());
            typeSymbol = typeSymbol.BaseType;
        }

        return result;
    }
}
