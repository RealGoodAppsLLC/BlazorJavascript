// <copyright file="JSObjectConstructorAttribute.cs" company="Real Good Apps">
// Copyright (c) Real Good Apps. All rights reserved.
// </copyright>

using System;
using RealGoodApps.BlazorJavascript.Interop.BuiltIns;

namespace RealGoodApps.BlazorJavascript.Interop.Attributes
{
    /// <summary>
    /// Attribute used to annotation interfaces that extend <see cref="IJSObject"/> which specifies the type that should
    /// be constructed when the object factory attempts to return the interface.
    /// </summary>
    public class JSObjectConstructorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JSObjectConstructorAttribute"/> class.
        /// </summary>
        /// <param name="type">The type of object that should be constructed for the interface.</param>
        public JSObjectConstructorAttribute(Type type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Gets the type of object that should be constructed for the interface.
        /// This must be a type that implements the interface that applies the attribute.
        /// </summary>
        public Type Type { get; }
    }
}
