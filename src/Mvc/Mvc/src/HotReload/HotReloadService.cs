// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Primitives;

[assembly: MetadataUpdateHandler(typeof(Microsoft.AspNetCore.Mvc.HotReload.HotReloadService))]

namespace Microsoft.AspNetCore.Mvc.HotReload
{
    internal sealed class HotReloadService : IActionDescriptorChangeProvider, IDisposable
    {
        private readonly DefaultModelMetadataProvider? _modelMetadataProvider;
        private readonly DefaultControllerPropertyActivator? _controllerPropertyActivator;
        private readonly DefaultViewCompiler? _defaultViewCompiler;
        private readonly RazorViewEngine? _razorViewEngine;
        private CancellationTokenSource _tokenSource = new();

        public HotReloadService(
            IModelMetadataProvider modelMetadataProvider,
            IControllerPropertyActivator controllerPropertyActivator,
            IViewCompiler razorViewCompiler,
            IRazorViewEngine razorViewEngine)
        {
            ClearCacheEvent += NotifyClearCache;
            if (modelMetadataProvider.GetType() == typeof(DefaultModelMetadataProvider))
            {
                _modelMetadataProvider = (DefaultModelMetadataProvider)modelMetadataProvider;
            }

            if (controllerPropertyActivator is DefaultControllerPropertyActivator defaultControllerPropertyActivator)
            {
                _controllerPropertyActivator = defaultControllerPropertyActivator;
            }

            if (razorViewCompiler is DefaultViewCompiler defaultViewCompiler)
            {
                _defaultViewCompiler = defaultViewCompiler;
            }

            if (razorViewEngine.GetType() == typeof(RazorViewEngine))
            {
                _razorViewEngine = (RazorViewEngine)razorViewEngine;
            }
        }

        public static event Action<Type[]?>? ClearCacheEvent;

        public static void ClearCache(Type[]? changedTypes)
        {
            ClearCacheEvent?.Invoke(changedTypes);
        }

        IChangeToken IActionDescriptorChangeProvider.GetChangeToken() => new CancellationChangeToken(_tokenSource.Token);

        private void NotifyClearCache(Type[]? changedTypes)
        {
            // Trigger the ActionDescriptorChangeProvider
            var current = Interlocked.Exchange(ref _tokenSource, new CancellationTokenSource());
            current.Cancel();

            // Clear individual caches
            _modelMetadataProvider?.ClearCache();
            _controllerPropertyActivator?.ClearCache();
            _defaultViewCompiler?.ClearCache(changedTypes);
            _razorViewEngine?.ClearCache();
        }

        public void Dispose()
        {
            ClearCacheEvent -= NotifyClearCache;
        }
    }
}
