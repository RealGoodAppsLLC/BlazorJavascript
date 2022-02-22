using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record FunctionTypeInfo(
        ExtractTypeParametersResult ExtractTypeParametersResult,
        ValueImmutableList<ParameterInfo> Parameters,
        TypeInfo ReturnType);
}
