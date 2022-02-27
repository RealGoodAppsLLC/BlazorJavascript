using RealGoodApps.BlazorJavascript.Interop.Interfaces;

namespace RealGoodApps.BlazorJavascript.Interop.Extensions
{
    public static class INumberExtensions
    {
        public static double ConvertToDotNetDouble(
            this INumber self)
        {
            return self.ConvertToValue<double>();
        }

        public static float ConvertToDotNetFloat(
            this INumber self)
        {
            return self.ConvertToValue<float>();
        }

        public static int ConvertToDotNetInt(
            this INumber self)
        {
            return self.ConvertToValue<int>();
        }

        public static long ConvertToDotNetLong(
            this INumber self)
        {
            return self.ConvertToValue<long>();
        }
    }
}
