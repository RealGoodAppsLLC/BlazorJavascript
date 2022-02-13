using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record ExtractTypeParametersResult(
        ImmutableList<TypeParameter> TypeParameters,
        bool AnyConstraintsAreNotSimple);
}
