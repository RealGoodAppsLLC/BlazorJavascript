namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models.Processed
{
    public record ProcessedConstructorInfo(
        ProcessedReturnTypeInfo ReturnType,
        string ConstructorName,
        ProcessedTypeParametersInfo TypeParameters,
        ProcessedParametersInfo Parameters);
}
