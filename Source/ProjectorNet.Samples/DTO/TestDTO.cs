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

    public string NullString { get; set; }

    public string NotNullString { get; set; }

    public long? IntToLong { get; set; }
}
