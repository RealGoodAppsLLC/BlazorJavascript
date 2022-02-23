using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models.Processed
{
    public sealed record ProcessedClassesInfo(
        ValueImmutableList<ProcessedClassInfo> Items);
}
