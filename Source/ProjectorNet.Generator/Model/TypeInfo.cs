using System;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.TypeKind;

namespace ProjectorNet.Generator.Model;

public record TypeInfo(TypeName Name, TypeKind Kind)
{
    public static TypeInfo FromSymbol(INamedTypeSymbol symbol)
    {
        if (symbol == null) throw new ArgumentNullException(nameof(symbol));

        return new TypeInfo(
            TypeName.FromSymbol(symbol),
            (symbol.TypeKind, symbol.IsRecord, symbol.IsReadOnly) switch
            {
                (Class, false, _) => TypeKind.Class,
                (Class, true, _) => TypeKind.RecordClass,
                (Struct, false, false) t => TypeKind.Struct,
                (Struct, true, false) => TypeKind.RecordStruct,
                (Struct, false, true) => TypeKind.ReadonlyStruct,
                (Struct, true, true) => TypeKind.ReadonlyRecordStruct,
                var (kind, _, _) => throw new ArgumentException($"Unsupported type kind {kind}", nameof(symbol))
            }
        );
    }
}
