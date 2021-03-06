// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class UnprocessableEntityResultTests
    {
        [Fact]
        public void UnprocessableEntityResult_InitializesStatusCode()
        {
            // Arrange & act
            var result = new UnprocessableEntityResult();

            // Assert
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
        }
    }
}
