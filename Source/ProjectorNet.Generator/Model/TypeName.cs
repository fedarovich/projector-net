using System;
using Microsoft.CodeAnalysis;

namespace ProjectorNet.Generator.Model;

public readonly record struct TypeName(string Name, string Namespace, string FullyQualifiedName, NullableAnnotation NullableAnnotation = NullableAnnotation.None)
{
    public static TypeName FromSymbol(ISymbol symbol)
    {
        return symbol switch
        {
            null => throw new ArgumentNullException(nameof(symbol)),
            INamedTypeSymbol typeSymbol => new TypeName(
                typeSymbol.Name,
                typeSymbol.ContainingNamespace.ToDisplayString(),
                typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                typeSymbol.NullableAnnotation),
            IArrayTypeSymbol { ElementType: var elementType } arrayTypeSymbol => new TypeName(
                elementType.Name + "[]",
                elementType.ContainingNamespace.ToDisplayString(),
                arrayTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                arrayTypeSymbol.NullableAnnotation),
            _ => throw new ArgumentException($"The {nameof(symbol)} must be a {nameof(INamedTypeSymbol)}", nameof(symbol))
        };
    }
}
