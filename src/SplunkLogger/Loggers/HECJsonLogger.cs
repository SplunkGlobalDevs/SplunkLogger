using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Splunk.Loggers
{
    public class HECJsonLogger : HECBaseLogger, ILogger
    {
        public HECJsonLogger(string categoryName, LogLevel threshold, HttpClient httpClient, BatchManager batchManager, ILoggerFormatter loggerFormatter)
            : base(categoryName, threshold, httpClient, batchManager, loggerFormatter)
        {
        }

        public void Log<T>(LogLevel logLevel, EventId eventId, T state, Exception exception, Func<T, Exception, string> formatter)
        {
            SplunkJSONEntry formatedMessage = null;
            if (loggerFormatter != null)
                formatedMessage = loggerFormatter.FormatJson(logLevel, eventId, state, exception);
            else if (formatter != null)
                formatedMessage = new SplunkJSONEntry(formatter(state, exception));
            batchManager.Add(JObject.FromObject(formatedMessage));
        }
    }
}