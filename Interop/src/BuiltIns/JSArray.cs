using System;
using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.Extensions;
using RealGoodApps.BlazorJavascript.Interop.Factories;

namespace RealGoodApps.BlazorJavascript.Interop.BuiltIns
{
    public interface IJSArray : IJSObject
    {
        JSNumber length { get; }

        IJSObject? GetItemAtIndex(JSNumber index);

        JSNumber PushItem(IJSObject? item);
    }

    public interface IJSArray<TJSObject> : IJSArray, IJSObject
        where TJSObject : class, IJSObject
    {
        JSNumber length { get; }

        TJSObject? GetItemAtIndex(JSNumber index);

        JSNumber PushItem(TJSObject? item);
    }

    public class JSArray : IJSArray, IJSObject
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

        public JSNumber length => JSArrayHelpers.GetLengthPlain(this);

        public IJSObject? GetItemAtIndex(JSNumber index)
        {
            var itemResultObject = JSArrayHelpers.GetItemAtIndexPlain(
                Runtime,
                ObjectReference,
                index);

            return itemResultObject;
        }

        public JSNumber PushItem(IJSObject? item)
        {
            return JSArrayHelpers.PushItemPlain(
                Runtime,
                ObjectReference,
                item);
        }
    }

    public class JSArray<TJSObject> : IJSArray<TJSObject>, IJSArray, IJSObject
        where TJSObject : class, IJSObject
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

        public JSNumber length => JSArrayHelpers.GetLengthPlain(this);

        IJSObject? IJSArray.GetItemAtIndex(JSNumber index)
        {
            var itemResultObject = JSArrayHelpers.GetItemAtIndexPlain(
                Runtime,
                ObjectReference,
                index);

            return itemResultObject;
        }

        JSNumber IJSArray.PushItem(IJSObject? item)
        {
            return JSArrayHelpers.PushItemPlain(
                Runtime,
                ObjectReference,
                item);
        }

        public TJSObject? GetItemAtIndex(JSNumber index)
        {
            var itemResultObject = JSArrayHelpers.GetItemAtIndexPlain(
                Runtime,
                ObjectReference,
                index);

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

        public JSNumber PushItem(TJSObject? item)
        {
            return JSArrayHelpers.PushItemPlain(
                Runtime,
                ObjectReference,
                item);
        }
    }

    public static class JSArrayHelpers
    {
        public static IJSObject? GetItemAtIndexPlain(
            IJSInProcessRuntime jsInProcessRuntime,
            IJSObjectReference objectReference,
            JSNumber index)
        {
            var itemResult = jsInProcessRuntime.Invoke<IJSObjectReference?>(
                "__blazorJavascript_arrayItemAtIndex",
                objectReference,
                index.ObjectReference);

            if (itemResult == null)
            {
                return null;
            }

            var itemResultObject = JSObjectFactory.FromRuntimeObjectReference(
                jsInProcessRuntime,
                itemResult);

            if (itemResultObject == null)
            {
                return null;
            }

            return itemResultObject;
        }

        public static JSNumber PushItemPlain(
            IJSInProcessRuntime jsInProcessRuntime,
            IJSObjectReference objectReference,
            IJSObject? item)
        {
            var itemResult = jsInProcessRuntime.Invoke<IJSObjectReference?>(
                "__blazorJavascript_arrayPush",
                objectReference,
                item?.ObjectReference);

            if (itemResult == null)
            {
                throw new Exception("The array push method did not return a result.");
            }

            var itemResultAsObject = JSObjectFactory.FromRuntimeObjectReference(
                jsInProcessRuntime,
                itemResult);

            if (itemResultAsObject is not JSNumber itemResultAsNumber)
            {
                throw new InvalidCastException("The array push method did not return a JSNumber.");
            }

            return itemResultAsNumber;
        }

        public static JSNumber GetLengthPlain(IJSObject array)
        {
            var lengthProperty = array.GetPropertyOfObject("length");

            if (lengthProperty is not JSNumber lengthPropertyAsNumber)
            {
                throw new InvalidCastException("The length property was not a JS number.");
            }

            return lengthPropertyAsNumber;
        }
    }
}
