using System.Collections.Generic;
using System.Linq;
using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.Factories;

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

        public TJSObject? Invoke<TJSObject>(
            IJSObject? thisObject,
            params IJSObject?[]? args)
            where TJSObject : class, IJSObject
        {
            var allParams = new List<object?>
            {
                ObjectReference,
                thisObject?.ObjectReference,
            };

            if (args != null)
            {
                allParams.AddRange(args
                    .Select(arg => arg?.ObjectReference)
                    .ToList());
            }

            var returnValueObjectReference = Runtime.Invoke<IJSObjectReference?>(
                "__blazorJavascript_invokeFunction",
                allParams.ToArray());

            return JSObjectFactory.CreateFromRuntimeObjectReference<TJSObject>(Runtime, returnValueObjectReference);
        }

        public void InvokeVoid(
            IJSObject? thisObject,
            params IJSObject?[]? args)
        {
            var allParams = new List<object?>
            {
                ObjectReference,
                thisObject?.ObjectReference,
            };

            if (args != null)
            {
                allParams.AddRange(args
                    .Select(arg => arg?.ObjectReference)
                    .ToList());
            }

            Runtime.InvokeVoid(
                "__blazorJavascript_invokeFunction",
                allParams.ToArray());
        }
    }
}
