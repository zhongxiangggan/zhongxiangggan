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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;

[assembly: MetadataUpdateHandler(typeof(Microsoft.AspNetCore.Mvc.HotReload.HotReloadService))]

namespace Microsoft.AspNetCore.Mvc.HotReload
{
    internal sealed class HotReloadService : IActionDescriptorChangeProvider, IDisposable
    {
        private readonly DefaultModelMetadataProvider? _modelMetadataProvider;
        private readonly DefaultControllerPropertyActivator? _controllerPropertyActivator;
        private readonly DefaultViewCompilerProvider? _defaultViewCompilerCompiler;
        private readonly RazorViewEngine? _razorViewEngine;
        private readonly RazorPageActivator? _razorPageActivator;
        private readonly DefaultTagHelperFactory? _defaultTagHelperFactory;
        private readonly TagHelperComponentPropertyActivator? _tagHelperComponentPropertyActivator;
        private CancellationTokenSource _tokenSource = new();

        public HotReloadService(
            IServiceProvider serviceProvider,
            IModelMetadataProvider modelMetadataProvider,
            IControllerPropertyActivator controllerPropertyActivator)
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

            // For Razor view services, use the service locator pattern because they views not be registered by default.
            if (serviceProvider.GetService<IViewCompilerProvider>() is DefaultViewCompilerProvider defaultViewCompilerCompiler)
            {
                _defaultViewCompilerCompiler = defaultViewCompilerCompiler;
            }

            if (serviceProvider.GetService<IRazorViewEngine>() is { } viewEngine && viewEngine.GetType()  == typeof(RazorViewEngine))
            {
                _razorViewEngine = (RazorViewEngine)viewEngine;
            }

            if (serviceProvider.GetService<IRazorPageActivator>() is { } razorPageActivator && razorPageActivator.GetType() == typeof(RazorPageActivator))
            {
                _razorPageActivator = (RazorPageActivator)razorPageActivator;
            }

            if (serviceProvider.GetService<ITagHelperFactory>() is DefaultTagHelperFactory defaultTagHelperFactory)
            {
                _defaultTagHelperFactory = defaultTagHelperFactory;
            }

            if (serviceProvider.GetService<ITagHelperComponentPropertyActivator>() is TagHelperComponentPropertyActivator tagHelperComponentPropertyActivator)
            {
                _tagHelperComponentPropertyActivator = tagHelperComponentPropertyActivator;
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
            if (_defaultViewCompilerCompiler?.GetCompiler() is DefaultViewCompiler compiler)
            {
                compiler.ClearCache(changedTypes);
            }

            _razorViewEngine?.ClearCache();
            _razorPageActivator?.ClearCache();
            _defaultTagHelperFactory?.ClearCache();
            _tagHelperComponentPropertyActivator?.ClearCache();
        }

        public void Dispose()
        {
            ClearCacheEvent -= NotifyClearCache;
        }
    }
}
