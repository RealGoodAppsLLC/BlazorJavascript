using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.Extensions;

namespace RealGoodApps.BlazorJavascript.Interop.BuiltIns
{
    public sealed class JSString : IJSObject
    {
        public JSString(
            IJSInProcessRuntime jsInProcessRuntime,
            IJSObjectReference jsObjectReference)
        {
            Runtime = jsInProcessRuntime;
            ObjectReference = jsObjectReference;
        }

        public IJSInProcessRuntime Runtime { get; }
        public IJSObjectReference ObjectReference { get; }

        public string? ConvertToDotNetString()
        {
            return this.ConvertToValue<string>();
        }

        public bool EqualsString(JSString? other)
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

            return ConvertToDotNetString() == other.ConvertToDotNetString();
        }
    }
}
