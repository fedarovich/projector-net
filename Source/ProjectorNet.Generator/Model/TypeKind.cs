namespace ProjectorNet.Generator.Model;

public enum TypeKind
{
    Class = 0b000,
    Struct = 0b001,
    RecordClass = 0b010,
    RecordStruct = 0b011,
    ReadonlyStruct = 0b101,
    ReadonlyRecordStruct = 0b111
}
