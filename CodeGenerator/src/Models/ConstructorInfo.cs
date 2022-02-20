using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record ConstructorInfo(
        TypeInfo ReturnType,
        ExtractTypeParametersResult ExtractTypeParametersResult,
        ImmutableList<ParameterInfo> Parameters)
    {
        public string GetNameForCSharp()
        {
            // FIXME: Perhaps use a convention that is a little more descriptive.
            return "construct";
        }
    }
}
