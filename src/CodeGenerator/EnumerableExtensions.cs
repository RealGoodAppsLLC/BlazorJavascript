namespace RealGoodApps.BlazorJavascript.CodeGenerator;

public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(
        this IEnumerable<T?> self)
    {
        return self.Where(i => i != null).Select(i => i!);
    }
}
