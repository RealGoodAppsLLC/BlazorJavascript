using System.Collections.Generic;
using System.Linq;
using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.BuiltIns;
using RealGoodApps.BlazorJavascript.Interop.Factories;
using RealGoodApps.BlazorJavascript.Interop.Interfaces;

namespace RealGoodApps.BlazorJavascript.Interop.Extensions
{
    public static class IFunctionExtensions
    {
        public static TJSObject? Invoke<TJSObject>(
            this IFunction self,
            IJSObject? thisObject,
            params IJSObject?[] args)
            where TJSObject : class, IJSObject
        {
            var allParams = new List<object?>
            {
                self.ObjectReference,
                thisObject?.ObjectReference,
            };

            allParams.AddRange(args
                .Select(arg => arg?.ObjectReference)
                .ToList());

            var returnValueObjectReference = self.Runtime.Invoke<IJSObjectReference?>(
                "__blazorJavascript_invokeFunction",
                allParams.ToArray());

            return JSObjectFactory.CreateFromRuntimeObjectReference<TJSObject>(self.Runtime, returnValueObjectReference);
        }

        public static void InvokeVoid(
            this IFunction self,
            IJSObject? thisObject,
            params IJSObject?[] args)
        {
            var allParams = new List<object?>
            {
                self.ObjectReference,
                thisObject?.ObjectReference,
            };

            allParams.AddRange(args
                .Select(arg => arg?.ObjectReference)
                .ToList());

            self.Runtime.InvokeVoid(
                "__blazorJavascript_invokeFunction",
                allParams.ToArray());
        }
    }
}
