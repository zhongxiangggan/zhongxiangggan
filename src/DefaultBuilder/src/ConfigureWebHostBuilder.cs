// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// A non-buildable <see cref="IWebHostBuilder"/> for <see cref="WebApplicationBuilder"/>.
    /// Use <see cref="WebApplicationBuilder.Build"/> to build the <see cref="WebApplicationBuilder"/>.
    /// </summary>
    public sealed class ConfigureWebHostBuilder : IWebHostBuilder, IRejectStartup
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ConfigurationManager _configuration;
        private readonly IServiceCollection _services;

        private readonly WebHostBuilderContext _context;

        internal ConfigureWebHostBuilder(ConfigurationManager configuration, IWebHostEnvironment environment, IServiceCollection services)
        {
            _configuration = configuration;
            _environment = environment;
            _services = services;

            _context = new WebHostBuilderContext
            {
                Configuration = _configuration,
                HostingEnvironment = _environment
            };
        }

        IWebHost IWebHostBuilder.Build()
        {
            throw new NotSupportedException($"Call {nameof(WebApplicationBuilder)}.{nameof(WebApplicationBuilder.Build)}() instead.");
        }

        /// <inheritdoc />
        public IWebHostBuilder ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            var previousContentRoot = _configuration[WebHostDefaults.ContentRootKey];
            var previousWebRoot = _configuration[WebHostDefaults.ContentRootKey];
            var previousApplication = _configuration[WebHostDefaults.ApplicationKey];
            var previousEnvironment = _configuration[WebHostDefaults.EnvironmentKey];
            var previousHostingStartupAssemblies = _configuration[WebHostDefaults.HostingStartupAssembliesKey];
            var previousHostingStartupAssembliesExclude = _configuration[WebHostDefaults.HostingStartupExcludeAssembliesKey];

            // Run these immediately so that they are observable by the imperative code
            configureDelegate(_context, _configuration);

            if (_configuration[WebHostDefaults.WebRootKey] is string value && !string.Equals(previousWebRoot, value, StringComparison.OrdinalIgnoreCase))
            {
                // We allow changing the web root since it's based off the content root and typically
                // read after the host is built.
                _environment.WebRootPath = Path.Combine(_environment.ContentRootPath, value);
            }
            else if (!string.Equals(previousApplication, _configuration[WebHostDefaults.ApplicationKey], StringComparison.OrdinalIgnoreCase))
            {
                // Disallow changing any host configuration
                throw new NotSupportedException("The application name changed. Changing the host configuration is not supported.");
            }
            else if (!string.Equals(previousContentRoot, _configuration[WebHostDefaults.ContentRootKey], StringComparison.OrdinalIgnoreCase))
            {
                // Disallow changing any host configuration
                throw new NotSupportedException("The content root changed. Changing the host configuration is not supported.");
            }
            else if (!string.Equals(previousEnvironment, _configuration[WebHostDefaults.EnvironmentKey], StringComparison.OrdinalIgnoreCase))
            {
                // Disallow changing any host configuration
                throw new NotSupportedException("The environment changed. Changing the host configuration is not supported.");
            }
            else if (!string.Equals(previousHostingStartupAssemblies, _configuration[WebHostDefaults.HostingStartupAssembliesKey], StringComparison.OrdinalIgnoreCase))
            {
                // Disallow changing any host configuration
                throw new NotSupportedException("The hosting startup assemblies changed. Changing the host configuration is not supported.");
            }
            else if (!string.Equals(previousHostingStartupAssembliesExclude, _configuration[WebHostDefaults.HostingStartupExcludeAssembliesKey], StringComparison.OrdinalIgnoreCase))
            {
                // Disallow changing any host configuration
                throw new NotSupportedException("The hosting startup assemblies exclude list changed. Changing the host configuration is not supported.");
            }

            return this;
        }

        /// <inheritdoc />
        public IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
        {
            // Run these immediately so that they are observable by the imperative code
            configureServices(_context, _services);
            return this;
        }

        /// <inheritdoc />
        public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            return ConfigureServices((WebHostBuilderContext context, IServiceCollection services) => configureServices(services));
        }

        /// <inheritdoc />
        public string? GetSetting(string key)
        {
            return _configuration[key];
        }

        /// <inheritdoc />
        public IWebHostBuilder UseSetting(string key, string? value)
        {
            // All properties on IWebHostEnvironment are non-nullable.
            if (value is null)
            {
                return this;
            }

            if (string.Equals(key, WebHostDefaults.WebRootKey, StringComparison.OrdinalIgnoreCase))
            {
                // We allow changing the web root since it's based off the content root and typically
                // read after the host is built.
                _environment.WebRootPath = Path.Combine(_environment.ContentRootPath, value);
            }
            else if (string.Equals(key, WebHostDefaults.ApplicationKey, StringComparison.OrdinalIgnoreCase))
            {
                // Disallow changing any host configuration
                throw new NotSupportedException("The application name changed. Changing the host configuration is not supported.");
            }
            else if (string.Equals(key, WebHostDefaults.ContentRootKey, StringComparison.OrdinalIgnoreCase))
            {
                // Disallow changing any host configuration
                throw new NotSupportedException("The content root changed. Changing the host configuration is not supported.");
            }
            else if (string.Equals(key, WebHostDefaults.EnvironmentKey, StringComparison.OrdinalIgnoreCase))
            {
                // Disallow changing any host configuration
                throw new NotSupportedException("The environment changed. Changing the host configuration is not supported.");
            }
            else if (string.Equals(key, WebHostDefaults.HostingStartupAssembliesKey, StringComparison.OrdinalIgnoreCase))
            {
                // Disallow changing any host configuration
                throw new NotSupportedException("The hosting startup assemblies changed. Changing the host configuration is not supported.");
            }
            else if (string.Equals(key, WebHostDefaults.HostingStartupExcludeAssembliesKey, StringComparison.OrdinalIgnoreCase))
            {
                // Disallow changing any host configuration
                throw new NotSupportedException("The hosting startup assemblies exclude list changed. Changing the host configuration is not supported.");
            }

            // Set the configuration value after we've validated the key
            _configuration[key] = value;

            return this;
        }
    }
}
