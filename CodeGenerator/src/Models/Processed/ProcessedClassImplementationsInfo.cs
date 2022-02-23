using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models.Processed
{
    public sealed record ProcessedClassImplementationsInfo(
        ValueImmutableList<ProcessedClassImplementationInfo> Items);
}
