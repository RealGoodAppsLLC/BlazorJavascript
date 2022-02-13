using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record ConstructorInfo(
        TypeInfo ReturnType,
        ImmutableList<ParameterInfo> Parameters);
}
