using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record UnionTypeInfo(
        ImmutableList<TypeInfo> Types);
}
