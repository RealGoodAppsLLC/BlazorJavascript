using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.Attributes;

namespace RealGoodApps.BlazorJavascript.Interop.BuiltIns
{
    [JSObjectConstructor(typeof(JSObject))]
    public interface IJSObject
    {
        IJSInProcessRuntime Runtime { get; }
        IJSObjectReference? ObjectReference { get; }
    }

    public class JSObject : IJSObject
    {
        public JSObject(
            IJSInProcessRuntime jsInProcessRuntime,
            IJSObjectReference jsObjectReference)
        {
            Runtime = jsInProcessRuntime;
            ObjectReference = jsObjectReference;
        }

        public IJSInProcessRuntime Runtime { get; }
        public IJSObjectReference ObjectReference { get; }
    }
}
