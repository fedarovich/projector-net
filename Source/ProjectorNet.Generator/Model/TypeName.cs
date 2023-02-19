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
                typeSymbol.GetFullyQualifiedName(),
                typeSymbol.NullableAnnotation),
            IArrayTypeSymbol { ElementType: var elementType } arrayTypeSymbol => new TypeName(
                elementType.Name + "[]",
                elementType.ContainingNamespace.ToDisplayString(),
                arrayTypeSymbol.GetFullyQualifiedName(),
                arrayTypeSymbol.NullableAnnotation),
            _ => throw new ArgumentException($"The {nameof(symbol)} must be a {nameof(INamedTypeSymbol)}", nameof(symbol))
        };
    }

    public bool Equals(TypeName other)
    {
        return Name == other.Name && Namespace == other.Namespace && FullyQualifiedName == other.FullyQualifiedName;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (Name != null ? Name.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (FullyQualifiedName != null ? FullyQualifiedName.GetHashCode() : 0);
            return hashCode;
        }
    }
}