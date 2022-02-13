namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record PropertyInfo(
        string Name,
        bool IsReadonly,
        TypeInfo Type);
}
