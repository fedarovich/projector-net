using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectorNet.Samples.Entities;

public class TestEntity
{
    public int Id { get; set; }

    public string? NullString { get; set; }

    public string NotNullString { get; set; } = null!;

    public int? IntToLong { get; set; }
}
