using System.Collections.Generic;
using System.Linq;
using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.BuiltIns;
using RealGoodApps.BlazorJavascript.Interop.Factories;

namespace RealGoodApps.BlazorJavascript.Interop.Extensions
{
    public static partial class IJSObjectExtensions
    {
        public static IJSObjectReference? CallConstructorInternal(
            IJSObject self,
            params object?[]? args)
        {
            var allParams = new List<object?>
            {
                self.ObjectReference,
            };

            if (args != null)
            {
                allParams.AddRange(args
                    .Select(arg => arg.ExtractObjectReference())
                    .ToList());
            }

            return self.Runtime.Invoke<IJSObjectReference?>("__blazorJavascript_constructorFunction", allParams.ToArray());
        }

        public static IJSObject? GetPropertyOfObject(
            this IJSObject self,
            string propertyName)
        {
            if (self is JSUndefined)
            {
                return self;
            }

            var returnValue = self.Runtime.Invoke<IJSObjectReference?>("__blazorJavascript_getterFunction", self.ObjectReference, propertyName);
            return JSObjectFactory.FromRuntimeObjectReference(self.Runtime, returnValue);
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
                self.ObjectReference, propertyName,
                value.ExtractObjectReference());
        }
    }
}
