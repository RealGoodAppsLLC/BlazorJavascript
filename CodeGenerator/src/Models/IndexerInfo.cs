namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models
{
    public sealed record IndexerInfo(
        TypeInfo IndexType,
        string IndexName,
        TypeInfo ReturnType,
        bool IsReadonly);
}
