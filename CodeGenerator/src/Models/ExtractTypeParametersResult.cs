using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record ExtractTypeParametersResult(
        ValueImmutableList<TypeParameter> TypeParameters,
        bool AnyConstraintsAreNotSimple);
}
