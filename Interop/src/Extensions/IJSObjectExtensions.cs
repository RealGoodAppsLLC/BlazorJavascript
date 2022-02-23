using System.Collections.Generic;
using System.Linq;
using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.BuiltIns;
using RealGoodApps.BlazorJavascript.Interop.Factories;

namespace RealGoodApps.BlazorJavascript.Interop.Extensions
{
    public static class IJSObjectExtensions
    {
        public static TJSObject CallConstructor<TJSObject>(
            this IJSObject self,
            params IJSObject?[]? args)
            where TJSObject : class, IJSObject
        {
            var allParams = new List<object?>
            {
                self.ObjectReference,
            };

            if (args != null)
            {
                allParams.AddRange(args
                    .Select(arg => arg?.ObjectReference)
                    .ToList());
            }

            var constructedObjectReference = self.Runtime.Invoke<IJSObjectReference?>("__blazorJavascript_constructorFunction", allParams.ToArray());
            return JSObjectFactory.CreateFromRuntimeObjectReference<TJSObject>(self.Runtime, constructedObjectReference)!;
        }

        public static TJSObject UnsafeCastTo<TJSObject>(this IJSObject self)
            where TJSObject : class, IJSObject
        {
            return JSObjectFactory.CreateFromRuntimeObjectReference<TJSObject>(self.Runtime, self.ObjectReference)!;
        }

        public static TJSObject? GetPropertyOfObject<TJSObject>(
            this IJSObject self,
            string propertyName)
            where TJSObject : class, IJSObject
        {
            var returnValue = self.Runtime.Invoke<IJSObjectReference?>(
                "__blazorJavascript_getterFunction",
                self.ObjectReference,
                propertyName);
            return JSObjectFactory.CreateFromRuntimeObjectReference<TJSObject>(self.Runtime, returnValue);
        }

        public static TJSObject? GetIndexerOfObject<TJSObject>(
            this IJSObject self,
            IJSObject? index)
            where TJSObject : class, IJSObject
        {
            var returnValue = self.Runtime.Invoke<IJSObjectReference?>(
                "__blazorJavascript_indexerGetFunction",
                self.ObjectReference,
                index?.ObjectReference);
            return JSObjectFactory.CreateFromRuntimeObjectReference<TJSObject>(self.Runtime, returnValue);
        }

        public static TValue? ConvertToValue<TValue>(
            this IJSObject self)
        {
            return self.Runtime.Invoke<TValue?>("__blazorJavascript_evalFunction", self.ObjectReference);
        }

        public static void SetPropertyOfObject(
            this IJSObject self,
            string propertyName,
            IJSObject? value)
        {
            self.Runtime.InvokeVoid(
                "__blazorJavascript_setterFunction",
                self.ObjectReference,
                propertyName,
                value?.ObjectReference);
        }

        public static void SetIndexerOfObject(
            this IJSObject self,
            IJSObject? index,
            IJSObject? value)
        {
            self.Runtime.InvokeVoid(
                "__blazorJavascript_indexerSetFunction",
                self.ObjectReference,
                index?.ObjectReference,
                value?.ObjectReference);
        }
    }
}
