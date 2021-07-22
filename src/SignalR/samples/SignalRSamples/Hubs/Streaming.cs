// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalRSamples.Hubs
{
    public class Streaming : Hub
    {
        private readonly ShutdownNotification _sn;

        public Streaming(ShutdownNotification sn)
        {
            _sn = sn;
        }

        public async IAsyncEnumerable<int> AsyncEnumerableCounter(int count, double delay)
        {
            for (var i = 0; i < count; i++)
            {
                yield return i;
                await Task.Delay((int)delay, _sn.Token);
            }
        }

        public ChannelReader<int> ObservableCounter(int count, double delay)
        {
            var observable = Observable.Interval(TimeSpan.FromMilliseconds(delay))
                             .Select((_, index) => index)
                             .Take(count);

            return observable.AsChannelReader(Context.ConnectionAborted);
        }

        public ChannelReader<int> ChannelCounter(int count, int delay)
        {
            var channel = Channel.CreateUnbounded<int>();

            Task.Run(async () =>
            {
                try
                {
                    for (var i = 0; i < count; i++)
                    {
                        await channel.Writer.WriteAsync(i, _sn.Token);
                        await Task.Delay(delay, _sn.Token);
                    }
                }
                finally
                {
                    channel.Writer.TryComplete();
                }
            });

            return channel.Reader;
        }
    }
}
