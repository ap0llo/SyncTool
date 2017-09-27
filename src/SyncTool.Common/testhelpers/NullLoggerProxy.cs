using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SyncTool.Common.TestHelpers
{
    public sealed class NullLogggerProxy<TCategoryName> : ILogger<TCategoryName>
    {
        readonly ILogger m_InnerLogger;

        public NullLogggerProxy() => m_InnerLogger = NullLogger<TCategoryName>.Instance;

        public IDisposable BeginScope<TState>(TState state) => m_InnerLogger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => m_InnerLogger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>             
            m_InnerLogger.Log(logLevel, eventId, state, exception, formatter);
    }
}
