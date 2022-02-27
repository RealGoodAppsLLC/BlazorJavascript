using RealGoodApps.BlazorJavascript.Interop.Interfaces;

namespace RealGoodApps.BlazorJavascript.Interop.Extensions
{
    public static class IStringExtensions
    {
        public static string? ConvertToDotNetString(
            this IString self)
        {
            return self.ConvertToValue<string>();
        }
    }
}
