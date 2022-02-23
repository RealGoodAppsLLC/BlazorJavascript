namespace RealGoodApps.BlazorJavascript.CodeGenerator.Models.Processed
{
    public sealed record ProcessedInterfaceInfo(
        string InterfaceName,
        ProcessedTypeParametersInfo TypeParameters,
        string InterfaceConstructor,
        ProcessedExtendsChainInfo ExtendsChain,
        ProcessedConstructorsInfo Constructors,
        ProcessedMethodsInfo Methods,
        ProcessedPropertiesInfo Properties,
        ProcessedIndexersInfo Indexers);
}
