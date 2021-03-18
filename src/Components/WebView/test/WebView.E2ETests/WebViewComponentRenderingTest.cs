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
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Remote;
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
            // https://github.com/MicrosoftEdge/WebView2Feedback/issues/510#issuecomment-746785880
            var opts = new EdgeOptions { UseWebView = true, UseChromium = true };
            var root = "(omitted)";
            opts.BinaryLocation = root + @"\artifacts\bin\WinFormsTestApp\Debug\net6.0-windows\WinFormsTestApp.exe";
            var service = EdgeDriverService.CreateChromiumService(
                root + @"\src\Components\WebView\test\WebView.E2ETests\node_modules\selenium-standalone\.selenium\chromiumedgedriver\latest-x64-msedgedriver_bin",
                opts.BinaryLocation);
            var driver = new EdgeDriverWithLogs(service, opts);
            var logs = new RemoteLogs(driver);
            return Task.FromResult(((IWebDriver)driver, (ILogs)logs));
        }

        private class EdgeDriverWithLogs : EdgeDriver, ISupportsLogs
        {
            public EdgeDriverWithLogs(EdgeDriverService service, EdgeOptions options) : base(service, options)
            {
            }
        }
    }
}


namespace Microsoft.AspNetCore.Components.E2ETest.Infrastructure.ServerFixtures
{
    public class ServerFixture
    {
        // For some reason, Selenium refuses to navigate to https://0.0.0.0/, saying it's not a valid URL
        public Uri RootUri { get; } = new Uri("https://microsoft.com/");
    }

    public class ToggleExecutionModeServerFixture<TClientProgram> : ServerFixture
    {
        public ExecutionMode ExecutionMode => ExecutionMode.Client;
    }

    public enum ExecutionMode { Client, Server }
}
