using System;
using Microsoft.Extensions.Logging;

namespace Splunk
{
    public interface ILoggerFormatter
    {
        string Format<T>(LogLevel logLevel, EventId eventId, T state, Exception exception);
        SplunkJSONEntry FormatJson<T>(LogLevel logLevel, EventId eventId, T state, Exception exception);
    }
}