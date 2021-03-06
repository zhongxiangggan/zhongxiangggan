// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;
using Xunit;

namespace Microsoft.AspNetCore.Internal
{
    public class AspNetCoreTempDirectoryTests
    {
        [Fact]
        public void GetTempDirectory_Returns_Valid_Location()
        {
            var tempDirectory = AspNetCoreTempDirectory.TempDirectory;
            Assert.NotNull(tempDirectory);
            Assert.True(Directory.Exists(tempDirectory));
        }
    }
}
