using System.Collections.Generic;

namespace ProjectorNet.Generator;

public static class EnumerableExtensions
{
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable) => new(enumerable);

    public static void AddAll<T>(this ICollection<T> collection, IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            collection.Add(item);
        }
    }
}
