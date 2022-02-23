namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models.Processed
{
    public record ProcessedPropertyInfo(
        ProcessedReturnTypeInfo ReturnType,
        string PropertyName,
        string NativePropertyName, // This is the actual JS method name, since we might be an overload!
        ProcessedPropertyMode Mode);
}
