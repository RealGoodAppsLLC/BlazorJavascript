using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public record InterfaceBodyInfo(
        ImmutableList<ConstructorInfo> Constructors,
        ImmutableList<PropertyInfo> Properties,
        ImmutableList<MethodInfo> Methods,
        ImmutableList<IndexerInfo> Indexers,
        ImmutableList<GetAccessorInfo> GetAccessors,
        ImmutableList<SetAccessorInfo> SetAccessors);
}
