using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Splunk;

namespace VTEX.SampleWebAPI.Logging
{
    public class VTEXSplunkLoggerFormatter : ILoggerFormatter
    {
        const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        readonly string appVersion;
        readonly string host;

        public VTEXSplunkLoggerFormatter(string appVersion, string host)
        {
            this.appVersion = appVersion;
            this.host = host;
        }

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

        public SplunkJSONEntry FormatJson<T>(LogLevel logLevel, EventId eventId, T state, Exception exception)
        {
            return new SplunkJSONEntry(Format(logLevel, eventId, state, exception), 0, host, string.Empty, "Log");
        }

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
