using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.BuiltIns;
using RealGoodApps.BlazorJavascript.Interop.Factories;
using RealGoodApps.BlazorJavascript.Interop.Interfaces;

namespace RealGoodApps.BlazorJavascript.Interop.Extensions
{
    public static class IJSInProcessRuntimeExtensions
    {
        public static IWindow? GetWindow(this IJSInProcessRuntime jsRuntime)
        {
            var objectReference = jsRuntime.Invoke<IJSObjectReference?>("__blazorJavascript_getWindow");
            return JSObjectFactory.CreateFromRuntimeObjectReference<IWindow>(jsRuntime, objectReference);
        }

        public static TJSObject? GetGlobalObjectByName<TJSObject>(
            this IJSInProcessRuntime jsRuntime,
            string identifier)
            where TJSObject : class, IJSObject
        {
            var objectReference = jsRuntime.Invoke<IJSObjectReference?>("eval", identifier);
            return JSObjectFactory.CreateFromRuntimeObjectReference<TJSObject>(jsRuntime, objectReference);
        }

        public static IString? CreateString(
            this IJSInProcessRuntime jsRuntime,
            string? stringValue)
        {
            var stringObjectReference = jsRuntime.Invoke<IJSObjectReference?>("__blazorJavascript_constructString", stringValue);
            return JSObjectFactory.CreateFromRuntimeObjectReference<IString>(jsRuntime, stringObjectReference);
        }

        public static INumber CreatePositiveInfinity(this IJSInProcessRuntime jsRuntime)
        {
            var infinityObjectReference = jsRuntime.Invoke<IJSObjectReference?>("__blazorJavascript_constructPositiveInfinity");
            return JSObjectFactory.CreateFromRuntimeObjectReference<INumber>(jsRuntime, infinityObjectReference)!;
        }

        public static INumber CreateNegativeInfinity(this IJSInProcessRuntime jsRuntime)
        {
            var infinityObjectReference = jsRuntime.Invoke<IJSObjectReference?>("__blazorJavascript_constructNegativeInfinity");
            return JSObjectFactory.CreateFromRuntimeObjectReference<INumber>(jsRuntime, infinityObjectReference)!;
        }

        public static INumber CreateNaN(this IJSInProcessRuntime jsRuntime)
        {
            var nanObjectReference = jsRuntime.Invoke<IJSObjectReference?>("__blazorJavascript_constructNaN");
            return JSObjectFactory.CreateFromRuntimeObjectReference<INumber>(jsRuntime, nanObjectReference)!;
        }

        public static INumber CreateNumberFromDouble(
            this IJSInProcessRuntime jsRuntime,
            double value)
        {
            var numberObjectReference = jsRuntime.Invoke<IJSObjectReference?>("__blazorJavascript_constructNumberFromDouble", value);
            return JSObjectFactory.CreateFromRuntimeObjectReference<INumber>(jsRuntime, numberObjectReference)!;
        }

        public static INumber CreateNumberFromInt(
            this IJSInProcessRuntime jsRuntime,
            int value)
        {
            var numberObjectReference = jsRuntime.Invoke<IJSObjectReference?>("__blazorJavascript_constructNumberFromInt", value);
            return JSObjectFactory.CreateFromRuntimeObjectReference<INumber>(jsRuntime, numberObjectReference)!;
        }

        public static INumber CreateNumberFromFloat(
            this IJSInProcessRuntime jsRuntime,
            float value)
        {
            var numberObjectReference = jsRuntime.Invoke<IJSObjectReference?>("__blazorJavascript_constructNumberFromFloat", value);
            return JSObjectFactory.CreateFromRuntimeObjectReference<INumber>(jsRuntime, numberObjectReference)!;
        }

        public static IBoolean CreateBoolean(
            this IJSInProcessRuntime jsRuntime,
            bool value)
        {
            var booleanObjectReference = jsRuntime.Invoke<IJSObjectReference?>("__blazorJavascript_constructBoolean", value);
            return JSObjectFactory.CreateFromRuntimeObjectReference<IBoolean>(jsRuntime, booleanObjectReference)!;
        }

        public static IArray<IJSObject> CreateArray(
            this IJSInProcessRuntime jsRuntime)
        {
            var arrayObjectReference = jsRuntime.Invoke<IJSObjectReference?>("__blazorJavascript_constructArray");
            return JSObjectFactory.CreateFromRuntimeObjectReference<IArray<IJSObject>>(jsRuntime, arrayObjectReference)!;
        }

        public static IArray<TJSObject> CreateArray<TJSObject>(
            this IJSInProcessRuntime jsRuntime)
            where TJSObject : class, IJSObject
        {
            var arrayObjectReference = jsRuntime.Invoke<IJSObjectReference?>("__blazorJavascript_constructArray");
            return JSObjectFactory.CreateFromRuntimeObjectReference<IArray<TJSObject>>(jsRuntime, arrayObjectReference)!;
        }
    }
}
