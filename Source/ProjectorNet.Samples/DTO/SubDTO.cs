using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectorNet.Attributes;
using ProjectorNet.Samples.Entities;

namespace ProjectorNet.Samples.DTO;

[ProjectFrom(typeof(SubEntity))]
public partial class SubDTO
{
    public double Value { get; set; }
}
