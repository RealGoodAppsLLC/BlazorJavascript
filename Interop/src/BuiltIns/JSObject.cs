using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.Prototypes;

namespace RealGoodApps.BlazorJavascript.Interop.BuiltIns
{
    public interface IJSObject
    {
        IJSInProcessRuntime Runtime { get; }
        IJSObjectReference? ObjectReference { get; }
    }

    public class JSObject : IJSObject
    {
        public JSObject(
            IJSInProcessRuntime jsInProcessRuntime,
            IJSObjectReference jsObjectReference)
        {
            Runtime = jsInProcessRuntime;
            ObjectReference = jsObjectReference;
        }

        public IJSInProcessRuntime Runtime { get; }
        public IJSObjectReference ObjectReference { get; }

        public static IJSObject? FromRuntimeObjectReference(
            IJSInProcessRuntime jsInProcessRuntime,
            IJSObjectReference? objectReference)
        {
            var prototype = jsInProcessRuntime.Invoke<int>("__blazorJavascript_obtainPrototype", objectReference);

            switch (prototype)
            {
                case 0:
                    return null;
                case 1:
                    return new JSUndefined(jsInProcessRuntime);
            }

            var objectReferenceNotNull = objectReference!;

            return prototype switch
            {
                2 => new JSNumber(jsInProcessRuntime, objectReferenceNotNull),
                3 => new JSString(jsInProcessRuntime, objectReferenceNotNull),
                4 => new HTMLButtonElementPrototype(jsInProcessRuntime, objectReferenceNotNull),
                5 => new EventPrototype(jsInProcessRuntime, objectReferenceNotNull),
                7 => new DocumentPrototype(jsInProcessRuntime, objectReferenceNotNull),
                8 => new JSFunction(jsInProcessRuntime, objectReferenceNotNull),
                9 => new WindowPrototype(jsInProcessRuntime, objectReferenceNotNull),
                10 => new JSBoolean(jsInProcessRuntime, objectReferenceNotNull),
                _ => new JSObject(jsInProcessRuntime, objectReferenceNotNull),
            };
        }
    }
}
