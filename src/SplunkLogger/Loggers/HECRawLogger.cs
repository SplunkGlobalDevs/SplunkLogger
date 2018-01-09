using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Splunk.Loggers
{
    public class HECRawLogger : HECBaseLogger, ILogger
    {
        public HECRawLogger(string categoryName, LogLevel threshold, HttpClient httpClient, BatchManager batchManager, ILoggerFormatter loggerFormatter)
            : base(categoryName, threshold, httpClient, batchManager, loggerFormatter)
        {
        }

        public void Log<T>(LogLevel logLevel, EventId eventId, T state, Exception exception, Func<T, Exception, string> formatter)
        {
            string formatedMessage = string.Empty;
            if (loggerFormatter != null)
                formatedMessage = loggerFormatter.Format(logLevel, eventId, state, exception);
            else if (formatter != null)
                formatedMessage = formatter(state, exception);

            if (!string.IsNullOrWhiteSpace(formatedMessage))
                batchManager.Add(formatedMessage);
        }
    }
}