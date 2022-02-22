using System;

namespace RealGoodApps.BlazorJavascript.Interop.Attributes
{
    public class JSObjectConstructorAttribute : Attribute
    {
        public Type Type { get; }

        public JSObjectConstructorAttribute(Type type)
        {
            Type = type;
        }
    }
}
