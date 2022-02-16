namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record GlobalVariableInfo(
        string Name,
        InterfaceBodyInfo? InlineInterface,
        TypeInfo? Type);
}
