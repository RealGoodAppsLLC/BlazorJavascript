namespace RealGoodApps.BlazorJavascript.PagesGenerator.Models.Input
{
    // GlobalCount is deprecated, add it with ClassCount.
    // PrototypeCount is deprecated, add it with ClassCount.
    // AppendedGlobalsCount is deprecated, add it with both PropertyImplementationCount and InterfacePropertyCount.
    public record CodeGeneratorStats(
        int InterfaceCount,
        int ClassCount,
        int GlobalCount,
        int PrototypeCount,
        int ConstructorImplementationCount,
        int MethodImplementationCount,
        int PropertyImplementationCount,
        int IndexerImplementationCount,
        int InterfaceConstructorCount,
        int InterfaceMethodCount,
        int InterfacePropertyCount,
        int InterfaceIndexerCount,
        int AppendedGlobalsCount);
}
