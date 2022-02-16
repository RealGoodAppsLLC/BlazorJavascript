using Microsoft.JSInterop;

namespace RealGoodApps.BlazorJavascript.Interop.BuiltIns
{
    public sealed class JSUndefined : IJSObject
    {
        public JSUndefined(IJSInProcessRuntime jsInProcessRuntime)
        {
            Runtime = jsInProcessRuntime;
        }

        public IJSInProcessRuntime Runtime { get; }
        public IJSObjectReference? ObjectReference => null;
    }
}
