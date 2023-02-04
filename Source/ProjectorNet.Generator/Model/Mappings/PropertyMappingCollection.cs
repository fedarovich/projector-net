using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ProjectorNet.Generator.Model.Mappings;

public readonly struct PropertyMappingCollection : IReadOnlyList<PropertyMapping>, IEquatable<PropertyMappingCollection>
{
    private readonly ImmutableArray<PropertyMapping> _propertyMappings;

    public PropertyMappingCollection(ImmutableArray<PropertyMapping> propertyMappings)
    {
        _propertyMappings = propertyMappings;
    }

    public static implicit operator PropertyMappingCollection(ImmutableArray<PropertyMapping> propertyMappings) => new (propertyMappings);

    public IEnumerator<PropertyMapping> GetEnumerator()
    {
        return ((IEnumerable<PropertyMapping>)_propertyMappings).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_propertyMappings).GetEnumerator();
    }

    public int Count => _propertyMappings.Length;

    public PropertyMapping this[int index] => _propertyMappings[index];

    public bool Equals(PropertyMappingCollection other)
    {
        return StructuralComparisons.StructuralEqualityComparer.Equals(_propertyMappings, other._propertyMappings);
    }

    public override bool Equals(object obj)
    {
        return obj is PropertyMappingCollection other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StructuralComparisons.StructuralEqualityComparer.GetHashCode(_propertyMappings);
    }

    public static bool operator ==(PropertyMappingCollection left, PropertyMappingCollection right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PropertyMappingCollection left, PropertyMappingCollection right)
    {
        return !left.Equals(right);
    }
}
