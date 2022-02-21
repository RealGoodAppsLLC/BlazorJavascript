using System.Collections.Generic;
using System.Linq;
using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.Extensions;

namespace RealGoodApps.BlazorJavascript.Interop.BuiltIns
{
    public sealed partial class JSFunction : IJSObject
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

        private IJSObjectReference? InvokeInternal(
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

            return Runtime.Invoke<IJSObjectReference?>("__blazorJavascript_invokeFunction", allParams.ToArray());
        }
    }
}
