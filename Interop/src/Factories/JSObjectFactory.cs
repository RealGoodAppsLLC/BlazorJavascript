using System;
using System.Reflection;
using Microsoft.JSInterop;
using RealGoodApps.BlazorJavascript.Interop.Attributes;
using RealGoodApps.BlazorJavascript.Interop.BuiltIns;

namespace RealGoodApps.BlazorJavascript.Interop.Factories
{
    public static class JSObjectFactory
    {
        public static TJSObject? CreateFromRuntimeObjectReference<TJSObject>(
            IJSInProcessRuntime jsInProcessRuntime,
            IJSObjectReference? objectReference)
            where TJSObject : class, IJSObject
        {
            if (objectReference == null)
            {
                return null;
            }

            var constructorType = DetermineConstructorType<TJSObject>();

            if (constructorType == null)
            {
                throw new Exception($"The type provided ({typeof(TJSObject).Name}) does not have a constructor. If you introduced this, and it is an interface, make sure you decorate it with JSObjectConstructorAttribute.");
            }

            var instance = Activator.CreateInstance(constructorType, jsInProcessRuntime, objectReference);

            if (instance is not TJSObject instanceAsType)
            {
                throw new InvalidOperationException($"The constructor for {typeof(TJSObject).Name} ({constructorType.Name}) is not properly implemented, as did not return an instance of {typeof(TJSObject).Name}.");
            }

            return instanceAsType;
        }

        private static Type? DetermineConstructorType<TJSObject>()
            where TJSObject : class, IJSObject
        {
            var typeInfo = typeof(TJSObject);

            if (!typeInfo.IsInterface)
            {
                return typeInfo;
            }

            var jsConstructorAttribute = typeInfo.GetCustomAttribute<JSObjectConstructorAttribute>();

            if (jsConstructorAttribute == null)
            {
                return null;
            }

            var attributeType = jsConstructorAttribute.Type;

            return !jsConstructorAttribute.Type.ContainsGenericParameters
                ? attributeType
                : attributeType.MakeGenericType(typeInfo.GenericTypeArguments);
        }
    }
}
