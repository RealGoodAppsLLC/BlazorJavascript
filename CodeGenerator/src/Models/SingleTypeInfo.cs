using RealGoodApps.ValueImmutableCollections;

namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record SingleTypeInfo(
        string? Name,
        StringLiteralInfo? StringLiteral,
        BooleanLiteralInfo? BooleanLiteral,
        NumberLiteralInfo? NumberLiteral,
        ValueImmutableList<TypeInfo> TypeArguments,
        bool IsUnhandled);
}
