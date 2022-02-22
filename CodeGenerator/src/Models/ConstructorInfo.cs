using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record ConstructorInfo(
        TypeInfo ReturnType,
        ExtractTypeParametersResult ExtractTypeParametersResult,
        ValueImmutableList<ParameterInfo> Parameters)
    {
        public string GetNameForCSharp()
        {
            // FIXME: Perhaps use a convention that is a little more descriptive.
            return "construct";
        }
    }
}
