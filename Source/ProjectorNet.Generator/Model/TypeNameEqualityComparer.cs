using System.Collections.Generic;

namespace ProjectorNet.Generator.Model;

public class TypeNameEqualityComparer
{
    public static IEqualityComparer<TypeName> FullyQualifiedIgnoreNullability { get; } =
        new FullyQualifiedNameEqualityComparer();

    private sealed class FullyQualifiedNameEqualityComparer : IEqualityComparer<TypeName>
    {
        public bool Equals(TypeName x, TypeName y)
        {
            return x.FullyQualifiedName == y.FullyQualifiedName;
        }

        public int GetHashCode(TypeName obj)
        {
            return (obj.FullyQualifiedName != null ? obj.FullyQualifiedName.GetHashCode() : 0);
        }
    }
}
