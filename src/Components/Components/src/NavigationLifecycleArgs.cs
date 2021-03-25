// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Components.Routing
{
    /// <summary>
    /// 
    /// </summary>
    public class NavigationLifecycleArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="isNavigationIntercepted"></param>
        /// <param name="forceLoad"></param>
        /// <param name="cancellationToken"></param>
        public NavigationLifecycleArgs(string location, bool forceLoad, CancellationToken cancellationToken, bool isNavigationIntercepted = false)
        {
            Location = location;
            IsNavigationIntercepted = isNavigationIntercepted;
            ForceLoad = forceLoad;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets the location we are about to change to.
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// Gets a value that determines if navigation for the link was intercepted.
        /// </summary>
        public bool IsNavigationIntercepted { get; }

        /// <summary>
        /// Gets a value if the Forceload flag was set during a call to <see cref="NavigationManager.NavigateTo" /> 
        /// </summary>
        public bool ForceLoad { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public CancellationToken CancellationToken { get; }
    }
}
