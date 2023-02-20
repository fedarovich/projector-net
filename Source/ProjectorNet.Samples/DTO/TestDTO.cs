using ProjectorNet.Attributes;
using ProjectorNet.Samples.Context;
using ProjectorNet.Samples.Entities;

namespace ProjectorNet.Samples.DTO;

[ProjectFrom(typeof(TestEntity), ContextType = typeof(IIdContext))]
public partial class TestDTO
{
    public TestDTO(
        string id, 
        [ProjectionParameter(UseDefaultValue = true, DefaultValue = "qwerty")] string nullString,
        [CollectionProjection(CollectionType = CollectionType.Array, ItemExpression = "$source.ToUpper()")] ICollection<string> strings)
    {
        Id = id;
        NullString = nullString;
    }

    public string Id { get; set; }

    [ProjectionProperty(DefaultValue = "xyz")]
    public string NullString { get; set; }

    public string NotNullString { get; set; }

    [ProjectionProperty(SourceName = nameof(TestEntity.SubEntity))]
    public SubDTO SubDTO { get; set; }

    public long? IntToLong { get; set; }

    [ProjectionProperty(SourceName = nameof(TestEntity.SubEntities))]
    public IList<SubDTO> SubDTOs { get; set; }

    [CollectionProjection(CollectionType = CollectionType.Array)]
    public ICollection<string> Strings { get; set; }

    public string[] IntsToString { get; set; }
}