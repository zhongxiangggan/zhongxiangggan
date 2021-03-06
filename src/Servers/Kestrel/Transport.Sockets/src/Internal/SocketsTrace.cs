// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.Internal
{
    internal class SocketsTrace : ISocketsTrace
    {
        private static readonly LogDefineOptions SkipEnabledCheckLogOptions = new() { SkipEnabledCheck = true };

        // ConnectionRead: Reserved: 3

        private static readonly Action<ILogger, string, Exception?> _connectionPause =
            LoggerMessage.Define<string>(LogLevel.Debug, new EventId(4, "ConnectionPause"), @"Connection id ""{ConnectionId}"" paused.", SkipEnabledCheckLogOptions);

        private static readonly Action<ILogger, string, Exception?> _connectionResume =
            LoggerMessage.Define<string>(LogLevel.Debug, new EventId(5, "ConnectionResume"), @"Connection id ""{ConnectionId}"" resumed.", SkipEnabledCheckLogOptions);

        private static readonly Action<ILogger, string, Exception?> _connectionReadFin =
            LoggerMessage.Define<string>(LogLevel.Debug, new EventId(6, "ConnectionReadFin"), @"Connection id ""{ConnectionId}"" received FIN.", SkipEnabledCheckLogOptions);

        private static readonly Action<ILogger, string, string, Exception?> _connectionWriteFin =
            LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(7, "ConnectionWriteFin"), @"Connection id ""{ConnectionId}"" sending FIN because: ""{Reason}""", SkipEnabledCheckLogOptions);

        // ConnectionWrite: Reserved: 11

        // ConnectionWriteCallback: Reserved: 12

        private static readonly Action<ILogger, string, Exception?> _connectionError =
            LoggerMessage.Define<string>(LogLevel.Debug, new EventId(14, "ConnectionError"), @"Connection id ""{ConnectionId}"" communication error.", SkipEnabledCheckLogOptions);

        private static readonly Action<ILogger, string, Exception?> _connectionReset =
            LoggerMessage.Define<string>(LogLevel.Debug, new EventId(19, "ConnectionReset"), @"Connection id ""{ConnectionId}"" reset.", SkipEnabledCheckLogOptions);

        private readonly ILogger _logger;

        public SocketsTrace(ILogger logger)
        {
            _logger = logger;
        }

        public void ConnectionRead(SocketConnection connection, int count)
        {
            // Don't log for now since this could be *too* verbose.
            // Reserved: Event ID 3
        }

        public void ConnectionReadFin(SocketConnection connection)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _connectionReadFin(_logger, connection.ConnectionId, null);
            }
        }

        public void ConnectionWriteFin(SocketConnection connection, string reason)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _connectionWriteFin(_logger, connection.ConnectionId, reason, null);
            }
        }

        public void ConnectionWrite(SocketConnection connection, int count)
        {
            // Don't log for now since this could be *too* verbose.
            // Reserved: Event ID 11
        }

        public void ConnectionWriteCallback(SocketConnection connection, int status)
        {
            // Don't log for now since this could be *too* verbose.
            // Reserved: Event ID 12
        }

        public void ConnectionError(SocketConnection connection, Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _connectionError(_logger, connection.ConnectionId, ex);
            }
        }

        public void ConnectionReset(string connectionId)
        {
            _connectionReset(_logger, connectionId, null);
        }

        public void ConnectionReset(SocketConnection connection)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _connectionReset(_logger, connection.ConnectionId, null);
            }
        }

        public void ConnectionPause(SocketConnection connection)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _connectionPause(_logger, connection.ConnectionId, null);
            }
        }

        public void ConnectionResume(SocketConnection connection)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _connectionResume(_logger, connection.ConnectionId, null);
            }
        }

        public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => _logger.Log(logLevel, eventId, state, exception, formatter);
    }
}
