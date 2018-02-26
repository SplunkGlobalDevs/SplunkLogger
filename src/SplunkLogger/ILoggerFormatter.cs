using System;
using Microsoft.Extensions.Logging;

namespace Splunk
{
    /// <summary>
    /// Define a formatter capable to provide a custom text or json entry for log.
    /// </summary>
    public interface ILoggerFormatter
    {
        /// <summary>
        /// Format the specified logLevel, eventId, state and exception into log string entry.
        /// </summary>
        /// <returns>Formatted log string.</returns>
        /// <param name="categoryName">Log category name.</param>
        /// <param name="logLevel">Log level.</param>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="state">Log object state.</param>
        /// <param name="exception">Log exception.</param>
        /// <typeparam name="T">Log entry.</typeparam>
        string Format<T>(string categoryName, LogLevel logLevel, EventId eventId, T state, Exception exception);

        /// <summary>
        /// Formats the specified logLevel, eventId, state and exception into json entry.
        /// </summary>
        /// <returns>The json.</returns>
        /// <param name="categoryName">Log category name.</param>
        /// <param name="logLevel">Log level.</param>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="state">Log object state.</param>
        /// <param name="exception">Log exception.</param>
        /// <typeparam name="T">Log entry.</typeparam>
        SplunkJSONEntry FormatJson<T>(string categoryName, LogLevel logLevel, EventId eventId, T state, Exception exception);
    }
}