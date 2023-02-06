using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectorNet.Attributes;
using ProjectorNet.Samples.Context;
using ProjectorNet.Samples.Entities;

namespace ProjectorNet.Samples.DTO;

[ProjectFrom(typeof(TestEntity), ContextType = typeof(IIdContext))]
public partial class TestDTO
{
    public string Id { get; set; }

    [ProjectionProperty(DefaultValue = (DateTimeKind)(-500))]
    public string NullString { get; set; }

    public string NotNullString { get; set; }

    public long? IntToLong { get; set; }

    public List<string> Strings { get; set; }

    public string[] IntsToString { get; set; }
}

//[ProjectFrom(typeof(TestEntity), ContextType = typeof(IIdContext))]
//public partial record TestDTO(string Id, string NullString)
//{
//    public TestDTO(string id) : this(id, "")
//    {
//    }
//}


//public partial record TestDTO
//{
//    public TestDTO(int x) : this("", "")
//    {
//    }

//    public int Test { get; set; }
//}