using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.BuiltIns;

namespace RealGoodApps.BlazorJavascript.Interop.Extensions
{
    public static class IJSObjectExtensions
    {
        public static IJSObject? GetPropertyOfObject(
            this IJSObject self,
            string propertyName)
        {
            if (self is JSUndefined)
            {
                return self;
            }

            var returnValue = self.Runtime.Invoke<IJSObjectReference?>("__blazorJavascript_getterFunction", self.ObjectReference, propertyName);
            return JSObject.FromRuntimeObjectReference(self.Runtime, returnValue);
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
