// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using BasicTestApp;
using Microsoft.AspNetCore.Components.E2ETest.Infrastructure.ServerFixtures;
using Microsoft.AspNetCore.Components.E2ETest.Tests;
using Microsoft.AspNetCore.E2ETesting;
using Microsoft.AspNetCore.Testing;
using OpenQA.Selenium;
using Xunit;
using Xunit.Abstractions;

[assembly: AssemblyFixture(typeof(SeleniumStandaloneServer))]

namespace WebViewE2ETest
{
    public class WebViewComponentRenderingTest : ComponentRenderingTest, IClassFixture<WebViewBrowserFixture>
    {
        public WebViewComponentRenderingTest(
            WebViewBrowserFixture browserFixture,
            ToggleExecutionModeServerFixture<Program> serverFixture,
            ITestOutputHelper output)
            : base(browserFixture, serverFixture, output)
        {
        }
    }

    public class WebViewBrowserFixture : BrowserFixture
    {
        public WebViewBrowserFixture(IMessageSink diagnosticsMessageSink) : base(diagnosticsMessageSink)
        {
        }

        protected override Task<(IWebDriver browser, ILogs log)> CreateBrowserAsync(string context, ITestOutputHelper output)
        {
            return base.CreateBrowserAsync(context, output);
        }
    }
}


namespace Microsoft.AspNetCore.Components.E2ETest.Infrastructure.ServerFixtures
{
    public class ServerFixture
    {
        public Uri RootUri { get; } = new Uri("https://0.0.0.0/");
    }

    public class ToggleExecutionModeServerFixture<TClientProgram> : ServerFixture
    {
        public ExecutionMode ExecutionMode => ExecutionMode.Client;
    }

    public enum ExecutionMode { Client, Server }
}
