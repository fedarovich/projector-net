using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectorNet.Attributes;
using ProjectorNet.Samples.Context;
using ProjectorNet.Samples.Entities;

namespace ProjectorNet.Samples.DTO;

//[ProjectFrom(typeof(Person), ContextType = typeof(IIdContext))]
public partial class PersonDTO
{
    [ProjectionProperty(ConversionMethod = nameof(IIdContext.ConvertId))]
    public string Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public ICollection<string> Tags { get; set; } = null!;

    public int IntAsString { get; set; }

    public int? A { get; set; }

    public int B { get; set; }

    public int C { get; set; }
}
