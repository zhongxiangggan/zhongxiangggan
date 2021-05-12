// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.Components.Server.Circuits
{
    internal class RemoteJSDataStream : Stream
    {
        private readonly static Dictionary<Guid, RemoteJSDataStream> Instances = new();

        private readonly Guid _streamId;
        private readonly JSRuntime _runtime;
        private readonly IJSObjectReference _jsObject;
        private readonly long _maxLength;
        private readonly CancellationToken _cancellationToken;
        private readonly Stream _pipeReaderStream;
        private readonly Pipe _pipe;
        private bool _hasStarted;

        public static Task SupplyData(IAsyncEnumerable<byte[]> data, string id, long length)
        {
            if (!Instances.TryGetValue(Guid.Parse(id), out var instance))
            {
                throw new InvalidOperationException("There is no data stream with the given identifier. It may have already been disposed.");
            }

            return instance.SupplyData(data, length);
        }

        public RemoteJSDataStream(JSRuntime runtime, IJSObjectReference jsObject, long maxLength, CancellationToken cancellationToken)
        {
            _streamId = Guid.NewGuid();
            _runtime = runtime;
            _jsObject = jsObject;
            _maxLength = maxLength;
            _cancellationToken = cancellationToken;

            Instances.Add(_streamId, this);

            var maxBufferSize = 1024; // TODO: Make configurable?
            _pipe = new Pipe(new PipeOptions(pauseWriterThreshold: maxBufferSize, resumeWriterThreshold: maxBufferSize));
            _pipeReaderStream = _pipe.Reader.AsStream();
        }

        // TODO: Surely this should be IAsyncEnumerable<ReadOnlySequence<byte>> so we can pass through the
        // data without having to copy it into a temporary buffer. But trying this gives strange errors -
        // sometimes the "chunk" variable below has a negative length, even though the logic in BlazorPackHubProtocolWorker
        // never returns a corrupted item as far as I can tell.
        private async Task SupplyData(IAsyncEnumerable<byte[]> data, long length)
        {
            try
            {
                if (length > _maxLength)
                {
                    throw new InvalidOperationException($"The incoming data stream of length {length} exceeds the maximum length {_maxLength}.");
                }

                long bytesRead = 0;
                await foreach (var chunk in data)
                {
                    bytesRead += chunk.Length;
                    if (bytesRead > length)
                    {
                        throw new InvalidOperationException($"The incoming data stream declared a length {_maxLength}, but {bytesRead} bytes were read.");
                    }

                    const int maxChunkLength = 1024 * 1024; // TODO: Should we enforce a rule here, or do we just trust the SignalR max message size to enforce this? And does it do so for chunks within streams?
                    if (chunk.Length > maxChunkLength)
                    {
                        throw new InvalidOperationException($"The incoming stream chunk of length {chunk.Length} exceeds the limit of {maxChunkLength}.");
                    }

                    CopyToPipeWriter(chunk, _pipe.Writer);
                    _pipe.Writer.Advance((int)chunk.Length);
                    await _pipe.Writer.FlushAsync(_cancellationToken);
                }
            }
            catch (Exception e)
            {
                await _pipe.Writer.CompleteAsync(e);
                return;
            }

            await _pipe.Writer.CompleteAsync();
        }

        private static void CopyToPipeWriter(byte[] chunk, PipeWriter writer)
        {
            var pipeBuffer = writer.GetSpan((int)chunk.Length);
            chunk.CopyTo(pipeBuffer);
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _pipeReaderStream.Length;

        public override long Position
        {
            get => _pipeReaderStream.Position;
            set => throw new NotSupportedException();
        }

        public override void Flush()
            => throw new NotSupportedException();

        // TODO: Should we allow this? Seems like it should be technically possible.
        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException("Synchronous reads are not supported.");

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await EnsureStartedTransmission();
            return await _pipeReaderStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await EnsureStartedTransmission();
            return await _pipeReaderStream.ReadAsync(buffer, cancellationToken);
        }

        private ValueTask EnsureStartedTransmission()
        {
            // TODO: Rather than starting the transmission here, we should probably start it during the initial
            // setup (e.g., replace the constructor with an async factory method) so that we can find out the
            // length of the stream up front. Or maybe the length should be supplied on the original IJSDataReference.
            if (!_hasStarted)
            {
                _hasStarted = true;
                return _runtime.InvokeVoidAsync("Blazor._internal.sendJSDataStream", _jsObject, _streamId);
            }

            return ValueTask.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Instances.Remove(_streamId);
            }
        }
    }
}
