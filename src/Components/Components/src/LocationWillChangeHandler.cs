// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Components.Routing
{
    /// <summary>
    /// Provides an interface for implementationing a handler that will be invoked.
    /// </summary>
    public interface LocationWillChangeHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        ValueTask InvokeAsync(NavigationLifecycleArgs args);
    }
}
