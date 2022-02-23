namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models.Processed
{
    public record ProcessedMethodInfo(
        ProcessedReturnTypeInfo? ReturnType,
        string MethodName,
        string NativeMethodName, // This is the actual JS method name, since we might be an overload!
        ProcessedTypeParametersInfo TypeParameters,
        ProcessedParametersInfo Parameters);
}
