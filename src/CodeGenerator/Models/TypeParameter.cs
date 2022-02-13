namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record TypeParameter(
        string Name,
        TypeInfo? Constraint);
}
