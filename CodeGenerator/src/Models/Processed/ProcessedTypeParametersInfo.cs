using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models.Processed
{
    public sealed record ProcessedTypeParametersInfo(
        ValueImmutableList<ProcessedTypeParameterInfo> Items);
}
