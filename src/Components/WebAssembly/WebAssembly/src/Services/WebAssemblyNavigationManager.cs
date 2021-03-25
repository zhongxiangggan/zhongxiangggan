// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Routing;
using Interop = Microsoft.AspNetCore.Components.Web.BrowserNavigationManagerInterop;

namespace Microsoft.AspNetCore.Components.WebAssembly.Services
{
    /// <summary>
    /// Default client-side implementation of <see cref="NavigationManager"/>.
    /// </summary>
    internal class WebAssemblyNavigationManager : NavigationManager
    {
        private CancellationTokenSource? _handlerCancellationSource;
        private Task? _currentHandler;

        /// <summary>
        /// Gets the instance of <see cref="WebAssemblyNavigationManager"/>.
        /// </summary>
        public static WebAssemblyNavigationManager Instance { get; set; } = default!;

        public WebAssemblyNavigationManager(string baseUri, string uri)
        {
            Initialize(baseUri, uri);
        }

        public void SetLocation(string uri, bool isInterceptedLink)
        {
            Uri = uri;
            NotifyLocationChanged(isInterceptedLink);
        }

        /// <inheritdoc />
        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (_currentHandler != null && !_currentHandler.IsCompleted){
                _handlerCancellationSource?.Cancel();
            }

            _handlerCancellationSource = new CancellationTokenSource();
            _currentHandler = NotifyLocationWillChange(new NavigationLifecycleArgs(location: uri, forceLoad: forceLoad, cancellationToken: _handlerCancellationSource.Token));

            DefaultWebAssemblyJSRuntime.Instance.Invoke<object>(Interop.NavigateTo, uri, forceLoad);
        }
    }
}
