using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record IntersectionTypeInfo(
        ImmutableList<TypeInfo> Types);
}
