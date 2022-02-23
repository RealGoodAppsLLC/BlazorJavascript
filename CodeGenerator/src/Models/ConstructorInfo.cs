using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record ConstructorInfo(
        TypeInfo ReturnType,
        ExtractTypeParametersResult ExtractTypeParametersResult,
        ValueImmutableList<ParameterInfo> Parameters);
}
