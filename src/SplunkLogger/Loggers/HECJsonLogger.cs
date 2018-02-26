using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Splunk.Loggers
{
    /// <summary>
    /// Class used to send log to splunk as JSON via HEC.
    /// </summary>
    public class HECJsonLogger : HECBaseLogger, ILogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.Loggers.HECJsonLogger"/> class.
        /// </summary>
        /// <param name="categoryName">Category name.</param>
        /// <param name="httpClient">Http client.</param>
        /// <param name="batchManager">Batch manager.</param>
        /// <param name="loggerFormatter">Formatter instance.</param>
        public HECJsonLogger(string categoryName, HttpClient httpClient, BatchManager batchManager, ILoggerFormatter loggerFormatter)
            : base(categoryName, httpClient, batchManager, loggerFormatter)
        {
        }

        /// <summary>
        /// Method used to create log.
        /// </summary>
        /// <returns>The log.</returns>
        /// <param name="logLevel">Log level.</param>
        /// <param name="eventId">Log event identifier.</param>
        /// <param name="state">Log object state.</param>
        /// <param name="exception">Log Exception.</param>
        /// <param name="formatter">Log text formatter function.</param>
        public override void Log<T>(LogLevel logLevel, EventId eventId, T state, Exception exception, Func<T, Exception, string> formatter)
        {
            SplunkJSONEntry formatedMessage = null;
            if (loggerFormatter != null)
                formatedMessage = loggerFormatter.FormatJson(categoryName, logLevel, eventId, state, exception);
            else if (formatter != null)
                formatedMessage = new SplunkJSONEntry(formatter(state, exception));
            batchManager.Add(JObject.FromObject(formatedMessage));
        }
    }
}