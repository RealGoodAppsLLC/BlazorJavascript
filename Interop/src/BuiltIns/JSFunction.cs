using System.Collections.Generic;
using System.Linq;
using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.Extensions;

namespace RealGoodApps.BlazorJavascript.Interop.BuiltIns
{
    public sealed class JSFunction : IJSObject
    {
        public JSFunction(
            IJSInProcessRuntime jsInProcessRuntime,
            IJSObjectReference jsObjectReference)
        {
            Runtime = jsInProcessRuntime;
            ObjectReference = jsObjectReference;
        }

        public IJSInProcessRuntime Runtime { get; }
        public IJSObjectReference ObjectReference { get; }

        public IJSObject? Invoke(
            IJSObject? thisObject,
            params object?[]? args)
        {
            var allParams = new List<object?>
            {
                ObjectReference,
                thisObject?.ObjectReference,
            };

            if (args != null)
            {
                allParams.AddRange(args
                    .Select(arg => arg.ExtractObjectReference())
                    .ToList());
            }

            var result = Runtime.Invoke<IJSObjectReference>("__blazorJavascript_invokeFunction", allParams.ToArray());
            return JSObject.FromRuntimeObjectReference(Runtime, result);
        }
    }
}
