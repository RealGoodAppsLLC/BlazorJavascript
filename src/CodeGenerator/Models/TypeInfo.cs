namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record TypeInfo(
        UnionTypeInfo? Union,
        IntersectionTypeInfo? Intersection,
        TypeInfo? Parenthesized,
        SingleTypeInfo? Single,
        FunctionTypeInfo? Function);
}
