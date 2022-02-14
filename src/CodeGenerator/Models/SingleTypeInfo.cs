using System.Collections.Immutable;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record SingleTypeInfo(
        string? Name,
        StringLiteralInfo? StringLiteral,
        BooleanLiteralInfo? BooleanLiteral,
        NumberLiteralInfo? NumberLiteral,
        ImmutableList<TypeInfo> TypeArguments,
        bool IsUnhandled)
    {
        public string? GetNameForCSharp()
        {
            if (this.Name == "boolean")
            {
                return "bool";
            }

            return this.Name;
        }
    }
}
