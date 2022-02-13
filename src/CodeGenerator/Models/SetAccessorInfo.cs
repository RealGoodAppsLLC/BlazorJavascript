using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record SetAccessorInfo(
        string Name,
        ImmutableList<ParameterInfo> Parameters);
}
