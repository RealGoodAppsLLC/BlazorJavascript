using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record UnionTypeInfo(
        ValueImmutableList<TypeInfo> Types);
}
