using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectorNet.Attributes;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class ProjectionParameterAttribute : Attribute
{
    public string? SourceName { get; set; }

    public string? ConversionMethod { get; set; }

    public string? Expression { get; set; }

    public object? DefaultValue { get; set; }

    public bool UseDefaultValue { get; set; }
}
