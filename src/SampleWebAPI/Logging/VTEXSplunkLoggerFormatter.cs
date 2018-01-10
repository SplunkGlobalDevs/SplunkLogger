using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;
using Splunk;

namespace VTEX.SampleWebAPI.Logging
{
    /// <summary>
    /// This class contains all methods to format a Log event into a VTEX standard text.
    /// </summary>
    /// <remarks>
    /// At our Splunk environment at VTEX we expect certain standard of text to be processed and
    /// we also have field extraction rules to be applied.
    /// Thats the reason why we use a custom LoggerFormmater.
    /// </remarks>
    public class VTEXSplunkLoggerFormatter : ILoggerFormatter
    {
        const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        readonly string appVersion;
        readonly string host;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:VTEX.SampleWebAPI.Logging.VTEXSplunkLoggerFormatter"/> class.
        /// </summary>
        public VTEXSplunkLoggerFormatter()
        {
            appVersion = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion;
            host = GetHost();
        }

        /// <summary>
        /// Format the specified logLevel, eventId, state and exception into log string entry.
        /// </summary>
        /// <returns>Formatted log string.</returns>
        /// <param name="logLevel">Log level.</param>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="state">Log object state.</param>
        /// <param name="exception">Log exception.</param>
        /// <typeparam name="T">Log entry.</typeparam>
        public string Format<T>(LogLevel logLevel, EventId eventId, T state, Exception exception)
        {
            string log;
            if (state is VTEXSplunkEntry)
            {
                var splunkEntry = state as VTEXSplunkEntry;
                var dateTime = DateTime.UtcNow.ToString(DateTimeFormat);
                string extraData = string.Empty;
                if (splunkEntry.ExtraParameters != null && splunkEntry.ExtraParameters.Count > 0)
                    extraData = string.Join(" ", splunkEntry.ExtraParameters.Select(part => { return $"{part.Item1}=\"{part.Item2}\""; }));
                string account = splunkEntry.Account;
                if (string.IsNullOrWhiteSpace(splunkEntry.Account))
                    account = "-";
                log = string.Format($"{dateTime} VTEXLog,splunkmanager,{host},{GetVTEXEventLevel(logLevel)},{GetVTEXLogType(logLevel)},\"{splunkEntry.WorkflowType}\",\"{splunkEntry.WorkflowInstance}\",{account},{appVersion} {extraData}");
            }
            else
            {
                var eventSegment = "";
                if (!string.IsNullOrWhiteSpace(eventId.Name))
                    eventSegment = $"Event '{eventId.Name}' on {eventId.Id}";
                var exceptionSegment = "";
                if (exception != null)
                    exceptionSegment = $"Exception type: {exception.GetType().FullName}. Exception message: {exception.Message}";
                log = string.Format($"[{logLevel}] {eventSegment} {state.ToString()}. {exceptionSegment}");
            }
            return log;
        }

        /// <summary>
        /// Formats the specified logLevel, eventId, state and exception into json entry.
        /// </summary>
        /// <returns>The json.</returns>
        /// <param name="logLevel">Log level.</param>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="state">Log object state.</param>
        /// <param name="exception">Log exception.</param>
        /// <typeparam name="T">Log entry.</typeparam>
        public SplunkJSONEntry FormatJson<T>(LogLevel logLevel, EventId eventId, T state, Exception exception)
        {
            return new SplunkJSONEntry(Format(logLevel, eventId, state, exception), 0, host, string.Empty, "Log");
        }

        /// <summary>
        /// Method created to get AWS EC2 host Id, or set `dev` as host if AWS internal call fails.
        /// </summary>
        string GetHost()
        {
            string ec2Host = string.Empty;
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    TimeSpan timeSpan = new TimeSpan(0, 0, 5);
                    var cancellationTokenSource = new CancellationTokenSource((int)timeSpan.TotalMilliseconds);
                    httpClient.Timeout = timeSpan;
                    httpClient.BaseAddress = new Uri("http://169.254.169.254/latest/meta-data/");
                    ec2Host = httpClient
                        .GetAsync("instance-id", cancellationTokenSource.Token)
                        .Result
                        .Content
                        .ReadAsStringAsync()
                        .Result;
                }
            }
            catch
            {
                ec2Host = "dev";
            }
            return ec2Host;
        }

        /// <summary>
        /// Based on LogLevel we define the correspondent VTEX event level.
        /// </summary>
        /// <returns>VTEX event level.</returns>
        string GetVTEXEventLevel(LogLevel logLevel)
        {
            VTEXEventLevel eventLevel = VTEXEventLevel.Debug;
            switch (logLevel)
            {
                case LogLevel.Critical:
                    eventLevel = VTEXEventLevel.Critical;
                    break;
                case LogLevel.Warning:
                case LogLevel.Error:
                    eventLevel = VTEXEventLevel.Important;
                    break;
                case LogLevel.Information:
                    eventLevel = VTEXEventLevel.Default;
                    break;
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.None:
                    eventLevel = VTEXEventLevel.Debug;
                    break;
            }
            return eventLevel.ToString().ToLower();
        }

        /// <summary>
        /// Based on LogLevel we define the correspondent VTEX log type.
        /// </summary>
        /// <returns>VTEX log type.</returns>
        string GetVTEXLogType(LogLevel logLevel)
        {
            VTEXLogType logType = VTEXLogType.Info;
            switch (logLevel)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    logType = VTEXLogType.Error;
                    break;
                case LogLevel.Warning:
                    logType = VTEXLogType.Warning;
                    break;
                case LogLevel.Debug:
                case LogLevel.Information:
                case LogLevel.Trace:
                case LogLevel.None:
                    logType = VTEXLogType.Info;
                    break;
            }
            return logType.ToString().ToLower();
        }
    }
}