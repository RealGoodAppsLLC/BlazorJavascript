using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.Attributes;
using RealGoodApps.BlazorJavascript.Interop.Extensions;
using RealGoodApps.BlazorJavascript.Interop.Factories;

namespace RealGoodApps.BlazorJavascript.Interop.BuiltIns
{
    [JSObjectConstructor(typeof(JSArray))]
    public interface IJSArray : IJSObject
    {
        JSNumber length { get; }

        IJSObject? GetItemAtIndex(JSNumber index);

        JSNumber PushItem(IJSObject? item);
    }

    [JSObjectConstructor(typeof(JSArray<>))]
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
            return JSArrayHelpers.GetItemAtIndexPlain<IJSObject>(
                Runtime,
                ObjectReference,
                index);
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
            return JSArrayHelpers.GetItemAtIndexPlain<IJSObject>(
                Runtime,
                ObjectReference,
                index);
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
            return JSArrayHelpers.GetItemAtIndexPlain<TJSObject>(
                Runtime,
                ObjectReference,
                index);
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
        public static TJSObject? GetItemAtIndexPlain<TJSObject>(
            IJSInProcessRuntime jsInProcessRuntime,
            IJSObjectReference objectReference,
            JSNumber index)
            where TJSObject : class, IJSObject
        {
            var itemResult = jsInProcessRuntime.Invoke<IJSObjectReference?>(
                "__blazorJavascript_arrayItemAtIndex",
                objectReference,
                index.ObjectReference);

            return JSObjectFactory.CreateFromRuntimeObjectReference<TJSObject>(
                jsInProcessRuntime,
                itemResult);
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

            return JSObjectFactory.CreateFromRuntimeObjectReference<JSNumber>(
                jsInProcessRuntime,
                itemResult)!;
        }

        public static JSNumber GetLengthPlain(IJSObject array) => array.GetPropertyOfObject<JSNumber>("length")!;
    }
}
