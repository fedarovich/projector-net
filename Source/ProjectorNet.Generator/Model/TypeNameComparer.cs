using System;
using System.Collections.Generic;

namespace ProjectorNet.Generator.Model;

public sealed class TypeNameComparer : IComparer<TypeName>
{
    public static IComparer<TypeName> Instance { get; } = new TypeNameComparer();

    public int Compare(TypeName x, TypeName y)
    {
        return string.Compare(x.FullyQualifiedName, y.FullyQualifiedName, StringComparison.Ordinal);
    }
}