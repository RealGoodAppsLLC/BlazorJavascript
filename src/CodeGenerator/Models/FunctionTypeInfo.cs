using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record FunctionTypeInfo(
        ExtractTypeParametersResult ExtractTypeParametersResult,
        ImmutableList<ParameterInfo> Parameters,
        TypeInfo ReturnType);
}
