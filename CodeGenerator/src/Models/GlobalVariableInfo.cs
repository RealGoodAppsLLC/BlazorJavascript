using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record GlobalVariableInfo(
        string Name,
        bool HasPrototype,
        ImmutableList<ConstructorInfo> Constructors,
        ImmutableList<PropertyInfo> Properties);
}
