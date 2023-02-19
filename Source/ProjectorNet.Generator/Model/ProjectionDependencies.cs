#nullable enable
using System;
using System.Collections.Immutable;
using System.Linq;

namespace ProjectorNet.Generator.Model;

public class ProjectionDependencies : IEquatable<ProjectionDependencies>
{
    public required Projection Projection { get; init; }

    public required ImmutableDictionary<TypeName, Projection?>? Dependencies { get; init; }

    private int? _dependenciesHashCode = null;

    public bool Equals(ProjectionDependencies? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Projection.Equals(other.Projection) && DependenciesEqual(other.Dependencies);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ProjectionDependencies)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Projection.GetHashCode() * 397) ^ GetDependenciesHashCode();
        }
    }

    public static bool operator ==(ProjectionDependencies? left, ProjectionDependencies? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ProjectionDependencies? left, ProjectionDependencies? right)
    {
        return !Equals(left, right);
    }

    public void Deconstruct(out Projection projection, out ImmutableDictionary<TypeName, Projection?>? dependencies)
    {
        projection = Projection;
        dependencies = Dependencies;
    }

    private bool DependenciesEqual(ImmutableDictionary<TypeName, Projection?>? other)
    {
        if (Dependencies == null)
            return other == null;
        if (other == null)
            return false;
        if (Dependencies.Count != other.Count)
            return false;

        foreach (var pair in Dependencies)
        {
            if (!other.TryGetValue(pair.Key, out var otherValue) || !Equals(pair.Value, otherValue))
                return false;
        }

        return true;
    }

    private int GetDependenciesHashCode()
    {
        return _dependenciesHashCode ??= Calculate();

        int Calculate()
        {
            if (Dependencies == null)
                return 0;

            int hashCode = 0;
            foreach (var keyValuePair in Dependencies.OrderBy(x => x.Key, TypeNameComparer.Instance))
            {
                hashCode = (hashCode * 397) ^ keyValuePair.GetHashCode();
            }

            return hashCode;
        }
    }
}
