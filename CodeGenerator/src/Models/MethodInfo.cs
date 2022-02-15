using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record MethodInfo(
        string Name,
        ExtractTypeParametersResult ExtractTypeParametersResult,
        TypeInfo ReturnType,
        ImmutableList<ParameterInfo> Parameters)
    {
        public string GetNameForCSharp()
        {
            return ReservedKeywords.SanitizeName(this.Name, true);
        }
    }
}
