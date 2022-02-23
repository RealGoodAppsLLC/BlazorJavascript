namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models.Processed
{
    public sealed record ProcessedClassInfo(
        string ClassName,
        string InterfaceName,
        ProcessedTypeParametersInfo TypeParameters,
        ProcessedClassImplementationsInfo Implementations);
}
