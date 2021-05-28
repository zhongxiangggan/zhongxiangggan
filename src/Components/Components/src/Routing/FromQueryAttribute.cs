// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Components.Routing
{
    /// <summary>
    /// Indicates that a component parameter may be populated from a querystring value.
    /// This attribute is only valid when used on a [Parameter] property within a routable (@page) component.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FromQueryAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public FromQueryAttribute()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryParameterName"></param>
        public FromQueryAttribute(string queryParameterName)
        {
            if (string.IsNullOrEmpty(queryParameterName))
            {
                throw new ArgumentException($"'{nameof(queryParameterName)}' cannot be null or empty.", nameof(queryParameterName));
            }

            QueryParameterName = queryParameterName;
        }

        /// <summary>
        /// 
        /// </summary>
        public string? QueryParameterName { get; }
    }
}
