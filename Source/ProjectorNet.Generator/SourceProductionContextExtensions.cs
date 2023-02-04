using System;
using Microsoft.CodeAnalysis;

namespace ProjectorNet.Generator;

internal static class SourceProductionContextExtensions
{
    public static void AddSource(this in SourceProductionContext context, string hintName, Action<SourceTextBuilder> action)
    {
        var builder = new SourceTextBuilder();
        action(builder);
        context.AddSource(hintName, builder.ToString());
    }
}
