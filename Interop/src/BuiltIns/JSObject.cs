using Microsoft.JSInterop;

namespace RealGoodApps.BlazorJavascript.Interop.BuiltIns
{
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
