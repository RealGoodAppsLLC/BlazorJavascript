namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models.Processed
{
    public sealed record ProcessedClassImplementationInfo(
        string Prefix,
        ProcessedConstructorsInfo Constructors,
        ProcessedMethodsInfo Methods,
        ProcessedPropertiesInfo Properties,
        ProcessedIndexersInfo Indexers);
}
