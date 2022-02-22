using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public record InterfaceBodyInfo(
        ValueImmutableList<ConstructorInfo> Constructors,
        ValueImmutableList<PropertyInfo> Properties,
        ValueImmutableList<MethodInfo> Methods,
        ValueImmutableList<IndexerInfo> Indexers,
        ValueImmutableList<GetAccessorInfo> GetAccessors,
        ValueImmutableList<SetAccessorInfo> SetAccessors);
}
