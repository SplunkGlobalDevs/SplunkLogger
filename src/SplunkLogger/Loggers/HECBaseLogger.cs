using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
               
namespace Splunk.Loggers
{
    public abstract class HECBaseLogger
    {
        protected readonly ILoggerFormatter loggerFormatter;
        protected readonly BatchManager batchManager;

        readonly string categoryName;
        readonly LogLevel threshold;
        readonly HttpClient httpClient;

        public HECBaseLogger(string categoryName, LogLevel threshold, HttpClient httpClient, BatchManager batchManager, ILoggerFormatter loggerFormatter)
        {
            this.categoryName = categoryName;
            this.threshold = threshold;
            this.httpClient = httpClient;
            this.batchManager = batchManager;
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