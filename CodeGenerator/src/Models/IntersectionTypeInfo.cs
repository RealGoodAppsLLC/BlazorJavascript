using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record IntersectionTypeInfo(
        ValueImmutableList<TypeInfo> Types);
}
