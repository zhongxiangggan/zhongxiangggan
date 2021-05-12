// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;

namespace Microsoft.JSInterop
{
    /// <summary>
    /// 
    /// </summary>
    public interface IJSDataReference : IAsyncDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxAllowedSize"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default);
    }
}
