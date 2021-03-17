// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

using BasicTestApp;
using Microsoft.AspNetCore.Components.E2ETest.Infrastructure.ServerFixtures;
using Microsoft.AspNetCore.Components.E2ETest.Tests;
using Microsoft.AspNetCore.E2ETesting;
using Microsoft.AspNetCore.Testing;
using Xunit.Abstractions;

[assembly: AssemblyFixture(typeof(SeleniumStandaloneServer))]

namespace WebViewE2ETest
{
    public class WebViewComponentRenderingTest : ComponentRenderingTest
    {
        public WebViewComponentRenderingTest(
            BrowserFixture browserFixture,
            ToggleExecutionModeServerFixture<Program> serverFixture,
            ITestOutputHelper output)
            : base(browserFixture, serverFixture, output)
        {
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
