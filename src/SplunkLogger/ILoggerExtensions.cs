using System;
using Microsoft.Extensions.Logging;

namespace Splunk
{
    public static class ILoggerExtensions
    {
        public static void Log(this ILogger logger, LogLevel logLevel, EventId eventId, object message, Exception exception)
        {
            var stringMessage = string.Format("{0} {1}", 
                                        message != null ? message.ToString() : string.Empty, 
                                        exception != null ? exception.ToString() : string.Empty);
            logger.Log(logLevel, eventId, message, exception, (s, e) => { return stringMessage; });
        }
    }
}