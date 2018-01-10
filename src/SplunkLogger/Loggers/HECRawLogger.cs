using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Splunk.Loggers
{
    /// <summary>
    /// Class used to send log to splunk as RAW via HEC.
    /// </summary>
    public class HECRawLogger : HECBaseLogger, ILogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.Loggers.HECRawLogger"/> class.
        /// </summary>
        /// <param name="categoryName">Category name.</param>
        /// <param name="threshold">Threshold.</param>
        /// <param name="httpClient">Http client.</param>
        /// <param name="batchManager">Batch manager.</param>
        /// <param name="loggerFormatter">Formatter instance.</param>
        public HECRawLogger(string categoryName, LogLevel threshold, HttpClient httpClient, BatchManager batchManager, ILoggerFormatter loggerFormatter)
            : base(categoryName, threshold, httpClient, batchManager, loggerFormatter)
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