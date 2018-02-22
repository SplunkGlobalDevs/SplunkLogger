using System;
using Microsoft.Extensions.Logging;

namespace Splunk.Loggers
{
    /// <summary>
    /// Define a base logger class.
    /// </summary>
    public abstract class BaseLogger : ILogger
    {
        protected readonly ILoggerFormatter loggerFormatter;

        readonly string categoryName;
        readonly LogLevel threshold;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.Loggers.BaseLogger"/> class.
        /// </summary>
        /// <param name="categoryName">Category name.</param>
        /// <param name="threshold">Threshold.</param>
        /// <param name="loggerFormatter">Formatter instance.</param>
        public BaseLogger(string categoryName, LogLevel threshold, ILoggerFormatter loggerFormatter)
        {
            this.categoryName = categoryName;
            this.threshold = threshold;
            this.loggerFormatter = loggerFormatter;
        }

        /// <summary>
        /// Ises the enabled.
        /// </summary>
        /// <returns><c>true</c>, if log level is equal ou higher than threshold, <c>false</c> otherwise.</returns>
        /// <param name="logLevel">.Net Core Log level.</param>
        public bool IsEnabled(LogLevel logLevel)
        {
            return (int)logLevel >= (int)threshold;
        }

        /// <summary>
        /// Not implemented method. DO NOT USE IT.
        /// </summary>
        public IDisposable BeginScope<T>(T state)
        {
            return null;
        }

        public abstract void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter);
    }
}