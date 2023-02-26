using System;
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

    public static IReadOnlyDictionary<string, IPropertySymbol> GetProperties(this ITypeSymbol typeSymbol, bool inherited, Func<IPropertySymbol, bool> filter)
    {
        var result = new Dictionary<string, IPropertySymbol>();

        do
        {
            foreach (var propertySymbol in typeSymbol.GetMembers().OfType<IPropertySymbol>().Where(filter))
            {
                if (!result.ContainsKey(propertySymbol.Name))
                {
                    result.Add(propertySymbol.Name, propertySymbol);
                }
            }

            typeSymbol = typeSymbol.BaseType;
        } while (inherited && typeSymbol != null && typeSymbol.SpecialType != SpecialType.System_Object);

        return result;
    }
}
