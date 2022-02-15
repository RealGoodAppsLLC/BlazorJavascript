namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record TypeAliasInfo(
        string Name,
        ExtractTypeParametersResult ExtractTypeParametersResult,
        TypeInfo AliasType);
}
