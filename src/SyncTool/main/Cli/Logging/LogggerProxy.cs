using System;
using Microsoft.Extensions.Logging;

namespace SyncTool.Cli.Logging
{
    /// <summary>
    /// Implementation of <see cref="ILogger{TCategoryName}"/> that uses the logger factory to create a 
    /// logger for the specified category on initialization and redirects all classes to the created logger.
    /// 
    /// This allows creation and injection of loggers into services using Autofac:
    ///     var containerBuilder = new ContainerBuilder();
    ///     containerBuilder .RegisterGeneric(typeof(LogggerProxy<>)).As(typeof(ILogger<>));
    ///     
    /// Registering a factory method to do this is not possible as Autofac does not support contextual bindings
    /// </summary>
    public sealed class LogggerProxy<TCategoryName> : ILogger<TCategoryName>
    {
        readonly ILogger m_InnerLogger;

        public LogggerProxy(ILoggerFactory factory) => m_InnerLogger = factory.CreateLogger<TCategoryName>();

        public IDisposable BeginScope<TState>(TState state) => m_InnerLogger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => m_InnerLogger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>             
            m_InnerLogger.Log(logLevel, eventId, state, exception, formatter);
    }
}
