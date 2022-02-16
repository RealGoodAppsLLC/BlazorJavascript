using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.Extensions;

namespace RealGoodApps.BlazorJavascript.Interop.BuiltIns
{
    public sealed class JSBoolean : IJSObject
    {
        public JSBoolean(
            IJSInProcessRuntime jsInProcessRuntime,
            IJSObjectReference jsObjectReference)
        {
            Runtime = jsInProcessRuntime;
            ObjectReference = jsObjectReference;
        }

        public IJSInProcessRuntime Runtime { get; }
        public IJSObjectReference ObjectReference { get; }

        public bool ConvertToDotNetBool()
        {
            return this.ConvertToValue<bool>();
        }

        public bool EqualsBoolean(JSBoolean? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (Runtime.Equals(other.Runtime) && ObjectReference.Equals(other.ObjectReference))
            {
                return true;
            }

            return ConvertToDotNetBool() == other.ConvertToDotNetBool();
        }
    }
}
