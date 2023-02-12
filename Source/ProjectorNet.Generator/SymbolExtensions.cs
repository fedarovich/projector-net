using Microsoft.CodeAnalysis;

namespace ProjectorNet.Generator;

public static class SymbolExtensions
{
    public static string GetFullyQualifiedName(this ISymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}
