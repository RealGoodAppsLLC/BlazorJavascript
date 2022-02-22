using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record ParsedInfo(
        ValueImmutableList<GlobalVariableInfo> GlobalVariables,
        ValueImmutableList<InterfaceInfo> Interfaces,
        ValueImmutableList<TypeAliasInfo> TypeAliases);
}
