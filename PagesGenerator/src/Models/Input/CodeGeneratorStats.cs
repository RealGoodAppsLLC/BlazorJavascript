namespace RealGoodApps.BlazorJavascript.PagesGenerator.Models.Input
{
    public record CodeGeneratorStats(
        int InterfaceCount,
        int GlobalCount,
        int PrototypeCount,
        int ConstructorImplementationCount,
        int MethodImplementationCount,
        int PropertyImplementationCount,
        int InterfaceConstructorCount,
        int InterfaceMethodCount,
        int InterfacePropertyCount,
        int AppendedGlobalsCount);
}
