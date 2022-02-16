using RealGoodApps.BlazorJavascript.Interop.BuiltIns;

namespace RealGoodApps.BlazorJavascript.Interop.Extensions
{
    public static class ObjectExtensions
    {
        public static object? ExtractObjectReference(
            this object? self)
        {
            return self is not IJSObject selfJsObject
                ? self
                : selfJsObject.ObjectReference;
        }
    }
}
