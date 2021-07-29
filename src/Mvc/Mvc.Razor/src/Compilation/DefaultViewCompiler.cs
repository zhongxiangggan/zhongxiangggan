// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(Microsoft.AspNetCore.Mvc.Razor.Compilation.DefaultViewCompiler))]

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    /// <summary>
    /// Caches the result of runtime compilation of Razor files for the duration of the application lifetime.
    /// </summary>
    internal sealed class DefaultViewCompiler : IViewCompiler
    {
        private readonly ConcurrentDictionary<string, Task<CompiledViewDescriptor>> _compiledViews;
        private readonly ConcurrentDictionary<string, string> _normalizedPathCache;
        private readonly ILogger _logger;
        private static DefaultViewCompiler? _viewCompiler;

        public DefaultViewCompiler(
            IList<CompiledViewDescriptor> compiledViews,
            ILogger logger)
        {
            _viewCompiler = this;

            if (compiledViews == null)
            {
                throw new ArgumentNullException(nameof(compiledViews));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger;
            _normalizedPathCache = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

            // We need to validate that the all of the precompiled views are unique by path (case-insensitive).
            // We do this because there's no good way to canonicalize paths on windows, and it will create
            // problems when deploying to linux. Rather than deal with these issues, we just don't support
            // views that differ only by case.
            _compiledViews = new ConcurrentDictionary<string, Task<CompiledViewDescriptor>>(StringComparer.OrdinalIgnoreCase);

            foreach (var compiledView in compiledViews)
            {
                logger.ViewCompilerLocatedCompiledView(compiledView.RelativePath);

                if (!_compiledViews.ContainsKey(compiledView.RelativePath))
                {
                    // View ordering has precedence semantics, a view with a higher precedence was not
                    // already added to the list.
                    _compiledViews.TryAdd(compiledView.RelativePath, Task.FromResult(compiledView));
                }
            }

            if (_compiledViews.Count == 0)
            {
                logger.ViewCompilerNoCompiledViewsFound();
            }
        }

        // Invoked as part of a hot reload event.
        internal void ClearCache(Type[]? types)
        {
            if (types is null)
            {
                return;
            }

            foreach (var type in types)
            {
                if (type.GetCustomAttribute<RazorFileIdentifierAttribute>() is { } attribute)
                {
                    var found = _compiledViews.TryGetValue(attribute.Identifier, out var previous);
                    Debug.Assert(found, "We'll usually expect a reloadable view to replace an existing one.");
                    if (!found)
                    {
                        continue;
                    }

                    var compiledItem = new HotReloadRazorCompiledItem(previous!.Result.Item!, type);
                    var compiledViewDescriptor = new CompiledViewDescriptor(compiledItem);
                    _compiledViews[attribute.Identifier] = Task.FromResult(compiledViewDescriptor);
                }
            }
        }

        /// <inheritdoc />
        public Task<CompiledViewDescriptor> CompileAsync(string relativePath)
        {
            if (relativePath == null)
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            // Attempt to lookup the cache entry using the passed in path. This will succeed if the path is already
            // normalized and a cache entry exists.
            if (_compiledViews.TryGetValue(relativePath, out var cachedResult))
            {
                _logger.ViewCompilerLocatedCompiledViewForPath(relativePath);
                return cachedResult;
            }

            var normalizedPath = GetNormalizedPath(relativePath);
            if (_compiledViews.TryGetValue(normalizedPath, out cachedResult))
            {
                _logger.ViewCompilerLocatedCompiledViewForPath(normalizedPath);
                return cachedResult;
            }

            // Entry does not exist. Attempt to create one.
            _logger.ViewCompilerCouldNotFindFileAtPath(relativePath);
            return Task.FromResult(new CompiledViewDescriptor
            {
                RelativePath = normalizedPath,
                ExpirationTokens = Array.Empty<IChangeToken>(),
            });
        }

        private string GetNormalizedPath(string relativePath)
        {
            Debug.Assert(relativePath != null);
            if (relativePath.Length == 0)
            {
                return relativePath;
            }

            if (!_normalizedPathCache.TryGetValue(relativePath, out var normalizedPath))
            {
                normalizedPath = ViewPath.NormalizePath(relativePath);
                _normalizedPathCache[relativePath] = normalizedPath;
            }

            return normalizedPath;
        }

        private sealed class HotReloadRazorCompiledItem : RazorCompiledItem
        {
            private readonly RazorCompiledItem _previous;
            public HotReloadRazorCompiledItem(RazorCompiledItem previous, Type type)
            {
                _previous = previous;
                Type = type;
            }

            public override string Identifier => _previous.Identifier;
            public override string Kind => _previous.Kind;
            public override IReadOnlyList<object> Metadata => Type.GetCustomAttributes(inherit: true);
            public override Type Type { get; }
        }
    }
}
