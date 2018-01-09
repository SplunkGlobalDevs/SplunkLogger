using System;
using Microsoft.Extensions.Logging;

namespace Splunk.Loggers
{
    public abstract class BaseLogger
    {
        protected readonly ILoggerFormatter loggerFormatter;

        readonly string categoryName;
        readonly LogLevel threshold;

        public BaseLogger(string categoryName, LogLevel threshold, ILoggerFormatter loggerFormatter)
        {
            this.categoryName = categoryName;
            this.threshold = threshold;
            this.loggerFormatter = loggerFormatter;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (int)logLevel >= (int)threshold;
        }

        public IDisposable BeginScope<T>(T state)
        {
            return null;
        }
    }
}