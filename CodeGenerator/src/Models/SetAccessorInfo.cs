using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record SetAccessorInfo(
        string Name,
        ValueImmutableList<ParameterInfo> Parameters);
}
