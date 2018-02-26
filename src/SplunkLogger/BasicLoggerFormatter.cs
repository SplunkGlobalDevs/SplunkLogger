using System;
using Microsoft.Extensions.Logging;

namespace Splunk
{
    public class BasicLoggerFormatter : ILoggerFormatter
    {
        public string Format<T>(string categoryName, LogLevel logLevel, EventId eventId, T state, Exception exception)
        {
            return string.Format("{0}: [{1}] [{2}:{3}] {4} {5}",
                                 categoryName,
                                 logLevel.ToString(),
                                 eventId.Id,
                                 eventId.Name,
                                 state != null ? state.ToString() : string.Empty,
                                 exception != null ? exception.ToString() : string.Empty);
        }

        public SplunkJSONEntry FormatJson<T>(string categoryName, LogLevel logLevel, EventId eventId, T state, Exception exception)
        {
            string eventText = Format(categoryName, logLevel, eventId, state, exception);
            return new SplunkJSONEntry(eventText);
        }
    }
}