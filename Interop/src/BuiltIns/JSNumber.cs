using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.Extensions;

namespace RealGoodApps.BlazorJavascript.Interop.BuiltIns
{
    public sealed class JSNumber : IJSObject
    {
        public JSNumber(
            IJSInProcessRuntime jsInProcessRuntime,
            IJSObjectReference jsObjectReference)
        {
            Runtime = jsInProcessRuntime;
            ObjectReference = jsObjectReference;
        }

        public IJSInProcessRuntime Runtime { get; }
        public IJSObjectReference ObjectReference { get; }

        public bool IsNaN()
        {
            return Runtime.Invoke<bool>("__blazorJavascript_isNaN", ObjectReference);
        }

        public bool IsInfinity()
        {
            return Runtime.Invoke<bool>("__blazorJavascript_isInfinity", ObjectReference);
        }

        public bool IsPositiveInfinity()
        {
            return Runtime.Invoke<bool>("__blazorJavascript_isPositiveInfinity", ObjectReference);
        }

        public bool IsNegativeInfinity()
        {
            return Runtime.Invoke<bool>("__blazorJavascript_isNegativeInfinity", ObjectReference);
        }

        public bool IsFinite()
        {
            return Runtime.Invoke<bool>("__blazorJavascript_isFinite", ObjectReference);
        }

        public bool IsInteger()
        {
            return Runtime.Invoke<bool>("__blazorJavascript_isInteger", ObjectReference);
        }

        public double ConvertToDotNetDouble()
        {
            return this.ConvertToValue<double>();
        }

        public float ConvertToDotNetFloat()
        {
            return this.ConvertToValue<float>();
        }

        public int ConvertToDotNetInt()
        {
            return this.ConvertToValue<int>();
        }

        public bool EqualsNumber(JSNumber? other)
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

            var isNaN = IsNaN();
            var otherIsNaN = other.IsNaN();

            if (isNaN || otherIsNaN)
            {
                return isNaN == otherIsNaN;
            }

            var isPositiveInfinity = IsPositiveInfinity();
            var otherIsPositiveInfinity = other.IsPositiveInfinity();

            if (isPositiveInfinity || otherIsPositiveInfinity)
            {
                return isPositiveInfinity == otherIsPositiveInfinity;
            }

            var isNegativeInfinity = IsNegativeInfinity();
            var otherIsNegativeInfinity = other.IsNegativeInfinity();

            if (isNegativeInfinity || otherIsNegativeInfinity)
            {
                return isNegativeInfinity == otherIsNegativeInfinity;
            }

            // If this looks confusing, look above. Only +Infinity and -Infinity should be considered equal when compared directly.
            if (IsInfinity() || other.IsInfinity())
            {
                return false;
            }

            // At this point, I believe we can safely assume that the number is finite.
            if (IsInteger() && other.IsInteger())
            {
                return ConvertToDotNetInt() == other.ConvertToDotNetInt();
            }

            // FIXME: This is far from perfect, but it gives us a pretty reasonable starting point.
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return ConvertToDotNetDouble() == other.ConvertToDotNetDouble();
        }
    }
}
