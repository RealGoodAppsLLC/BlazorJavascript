using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record ParsedInfo(
        ImmutableList<GlobalVariableInfo> GlobalVariables,
        ImmutableList<InterfaceInfo> Interfaces);
}
