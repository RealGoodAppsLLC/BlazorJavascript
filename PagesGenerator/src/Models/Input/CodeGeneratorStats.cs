namespace RealGoodApps.BlazorJavascript.PagesGenerator.Models.Input
{
    public record CodeGeneratorStats(
        int InterfaceCount,
        int GlobalCount,
        int PrototypeCount,
        int MethodImplementationCount,
        int PropertyImplementationCount,
        int InterfaceMethodCount,
        int InterfacePropertyCount,
        int AppendedGlobalsCount);
}
