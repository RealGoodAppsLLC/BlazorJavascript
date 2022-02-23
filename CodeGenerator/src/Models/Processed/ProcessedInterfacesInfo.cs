using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models.Processed
{
    public sealed record ProcessedInterfacesInfo(
        ValueImmutableList<ProcessedInterfaceInfo> Items);
}
