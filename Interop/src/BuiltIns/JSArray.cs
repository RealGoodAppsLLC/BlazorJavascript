using System;
using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.Extensions;
using RealGoodApps.BlazorJavascript.Interop.Factories;

namespace RealGoodApps.BlazorJavascript.Interop.BuiltIns
{
    public class JSArray : IJSObject
    {
        public JSArray(
            IJSInProcessRuntime jsInProcessRuntime,
            IJSObjectReference jsObjectReference)
        {
            Runtime = jsInProcessRuntime;
            ObjectReference = jsObjectReference;
        }

        public IJSInProcessRuntime Runtime { get; }
        public IJSObjectReference ObjectReference { get; }

        public JSNumber length
        {
            get
            {
                var lengthProperty = this.GetPropertyOfObject(nameof(length));

                if (lengthProperty is not JSNumber lengthPropertyAsNumber)
                {
                    throw new InvalidCastException("The length property was not a JS number.");
                }

                return lengthPropertyAsNumber;
            }
        }

        public virtual IJSObject? GetItemAtIndex(JSNumber index)
        {
            var itemResult = Runtime.Invoke<IJSObjectReference?>(
                "__blazorJavascript_arrayItemAtIndex",
                ObjectReference,
                index);

            if (itemResult == null)
            {
                return null;
            }

            return JSObjectFactory.FromRuntimeObjectReference(
                Runtime,
                itemResult);
        }
    }

    public class JSArray<TJSObject> : JSArray
        where TJSObject : class, IJSObject
    {
        public JSArray(IJSInProcessRuntime jsInProcessRuntime, IJSObjectReference jsObjectReference)
            : base(jsInProcessRuntime, jsObjectReference)
        {
        }

        public override TJSObject? GetItemAtIndex(JSNumber index)
        {
            var itemAsJsObject = base.GetItemAtIndex(index);

            if (itemAsJsObject is not TJSObject itemAsResultType)
            {
                throw new InvalidCastException("The item retrieved is not the correct type.");
            }

            return itemAsResultType;
        }
    }
}
