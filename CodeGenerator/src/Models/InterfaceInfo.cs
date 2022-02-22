using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record InterfaceInfo(
        string Name,
        ExtractTypeParametersResult ExtractTypeParametersResult,
        ValueImmutableList<TypeInfo> ExtendsList,
        InterfaceBodyInfo Body);
}
