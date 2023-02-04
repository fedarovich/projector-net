#nullable enable

using System;
using Microsoft.CodeAnalysis;

namespace ProjectorNet.Generator.Model;

public readonly record struct PropertyInfo(TypeName Type, string Name)
{
    public static PropertyInfo FromSymbol(IPropertySymbol propertySymbol)
    {
        if (propertySymbol == null)
            throw new ArgumentNullException(nameof(propertySymbol));

        return new PropertyInfo(TypeName.FromSymbol(propertySymbol.Type), propertySymbol.Name);
    }
}
