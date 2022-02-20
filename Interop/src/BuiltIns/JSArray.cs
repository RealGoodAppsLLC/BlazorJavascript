using System;
using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.Extensions;
using RealGoodApps.BlazorJavascript.Interop.Factories;

namespace RealGoodApps.BlazorJavascript.Interop.BuiltIns
{
    public abstract class JSAbstractArray<TJSObject> : IJSObject
        where TJSObject : class, IJSObject
    {
        protected JSAbstractArray(
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

        public TJSObject? GetItemAtIndex(JSNumber index)
        {
            var itemResult = Runtime.Invoke<IJSObjectReference?>(
                "__blazorJavascript_arrayItemAtIndex",
                ObjectReference,
                index.ObjectReference);

            if (itemResult == null)
            {
                return null;
            }

            var itemResultObject = JSObjectFactory.FromRuntimeObjectReference(
                Runtime,
                itemResult);

            if (itemResultObject == null)
            {
                return null;
            }

            if (itemResultObject is not TJSObject itemResultAsType)
            {
                throw new InvalidCastException("The item at index method did not return the right type.");
            }

            return itemResultAsType;
        }

        public JSNumber PushItem(TJSObject item)
        {
            var itemResult = Runtime.Invoke<IJSObjectReference?>(
                "__blazorJavascript_arrayPush",
                ObjectReference,
                item.ObjectReference);

            if (itemResult == null)
            {
                throw new Exception("The array push method did not return a result.");
            }

            var itemResultAsObject = JSObjectFactory.FromRuntimeObjectReference(
                Runtime,
                itemResult);

            if (itemResultAsObject is not JSNumber itemResultAsNumber)
            {
                throw new InvalidCastException("The array push method did not return a JSNumber.");
            }

            return itemResultAsNumber;
        }
    }

    public class JSArray : JSAbstractArray<IJSObject>
    {
        public JSArray(
            IJSInProcessRuntime jsInProcessRuntime,
            IJSObjectReference jsObjectReference)
            : base(jsInProcessRuntime, jsObjectReference)
        {
        }
    }

    public class JSArray<TJSObject> : JSAbstractArray<TJSObject>
        where TJSObject : class, IJSObject
    {
        public JSArray(IJSInProcessRuntime jsInProcessRuntime, IJSObjectReference jsObjectReference)
            : base(jsInProcessRuntime, jsObjectReference)
        {
        }
    }
}
