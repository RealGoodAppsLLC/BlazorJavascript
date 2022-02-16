using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record InterfaceInfo(
        string Name,
        ExtractTypeParametersResult ExtractTypeParametersResult,
        ImmutableList<TypeInfo> ExtendsList,
        InterfaceBodyInfo Body);
}
