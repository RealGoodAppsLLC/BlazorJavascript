namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models.Processed
{
    public sealed record ProcessedSymbols(
        ProcessedConstructorsInfo Constructors,
        ProcessedMethodsInfo Methods,
        ProcessedPropertiesInfo Properties,
        ProcessedIndexersInfo Indexers);
}
