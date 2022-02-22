using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record MethodInfo(
        string Name,
        ExtractTypeParametersResult ExtractTypeParametersResult,
        TypeInfo ReturnType,
        ValueImmutableList<ParameterInfo> Parameters)
    {
        public string GetNameForCSharp()
        {
            return ReservedKeywords.SanitizeName(this.Name);
        }
    }
}
