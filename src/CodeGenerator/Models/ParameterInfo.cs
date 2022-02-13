namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record ParameterInfo(
        string Name,
        bool IsOptional,
        TypeInfo Type);
}
