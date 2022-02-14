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
                return "JSBoolean";
            }

            if (this.Name == "string")
            {
                return "JSString";
            }

            if (this.Name == "any")
            {
                return "IJSObject";
            }

            if (this.Name == "number")
            {
                return "JSNumber";
            }

            return this.Name;
        }
    }
}
