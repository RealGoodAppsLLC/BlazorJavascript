using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator;

public static class ValueImmutableListExtensions
{
    public static IEnumerable<T> DistinctSafeSlow<T>(
        this IEnumerable<T> self)
    {
        var newList = new List<T>();

        foreach (var item in self)
        {
            if (newList.Contains(item))
            {
                continue;
            }

            newList.Add(item);
        }

        return newList;
    }
}
