// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop.Infrastructure;

namespace Microsoft.JSInterop
{
    /// <summary>
    /// Identifies a .NET method as allowing invocation from JavaScript code.
    /// Any method marked with this attribute may receive arbitrary parameter values
    /// from untrusted callers. All inputs should be validated carefully.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class JSInvokableAttribute : Attribute
    {
        

        /// <summary>
        /// Constructs an instance of <see cref="JSInvokableAttribute"/> without setting
        /// an identifier for the method.
        /// </summary>
        public JSInvokableAttribute()
        {
            JsonSerializeDelegate = JsonSerializer.Deserialize;
        }

        /// <summary>
        /// Constructs an instance of <see cref="JSInvokableAttribute"/> using the specified
        /// identifier.
        /// </summary>
        /// <param name="identifier">An identifier for the method, which must be unique within the scope of the assembly.</param>
        public JSInvokableAttribute(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("Cannot be null or empty", nameof(identifier));
            }

            Identifier = identifier;
            JsonSerializeDelegate = JsonSerializer.Deserialize;
        }

        /// <summary>
        /// Constructs an instance of <see cref="JSInvokableAttribute"/> using the specified
        /// identifier.
        /// </summary>
        /// <param name="serializerContextType">The <see cref="Type"/> of the <see cref="JsonSerializerContext"/> used while reading the JS interop arguments.</param>
        /// <param name="identifier">An optional identifier for the method, which must be unique within the scope of the assembly.</param>
        public JSInvokableAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serializerContextType, string? identifier = null)
        {
            if (serializerContextType is null)
            {
                throw new ArgumentNullException(nameof(serializerContextType));
            }

            if (!typeof(JsonSerializerContext).IsAssignableFrom(serializerContextType) ||
                serializerContextType.GetConstructor(new[] { typeof(JsonSerializerOptions) }) is not ConstructorInfo constructor)
            {
                throw new ArgumentException($"Type specified by '{nameof(serializerContextType)}' must derive" +
                    $" from '{typeof(JsonSerializerContext)}' and must have a constructor that accepts {typeof(JsonSerializerOptions)}.");
            }

            Identifier = identifier;

            var cache = new ConcurrentDictionary<JsonSerializerOptions, JsonSerializerContext>();
            JsonSerializeDelegate = JsonDeserializeWithContext;

            object? JsonDeserializeWithContext(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
            {
                if (!cache.TryGetValue(options, out var context))
                {
                    context = (JsonSerializerContext)Activator.CreateInstance(serializerContextType, options)!;
                }

                return JsonSerializer.Deserialize(ref reader, type, context);
            }
        }

        /// <summary>
        /// Gets the identifier for the method. The identifier must be unique within the scope
        /// of an assembly.
        ///
        /// If not set, the identifier is taken from the name of the method. In this case the
        /// method name must be unique within the assembly.
        /// </summary>
        public string? Identifier { get; }

        internal DotNetDispatcher.JsonDeserializeDelegate JsonSerializeDelegate { get; }
    }
}
