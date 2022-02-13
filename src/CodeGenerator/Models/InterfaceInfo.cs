using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record InterfaceInfo(
        string Name,
        ExtractTypeParametersResult ExtractTypeParametersResult,
        ImmutableList<string> ExtendsList,
        ImmutableList<PropertyInfo> Properties,
        ImmutableList<MethodInfo> Methods,
        ImmutableList<IndexerInfo> Indexers,
        ImmutableList<GetAccessorInfo> GetAccessors,
        ImmutableList<SetAccessorInfo> SetAccessors);
}
