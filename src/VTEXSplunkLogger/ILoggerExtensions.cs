using System;
using Microsoft.Extensions.Logging;
using Vtex.SplunkLogger;

namespace Vtex
{
    /// <summary>
    /// This class contains ILogger extension method to simplify the process to record a VTEX log.
    /// </summary>
    public static class ILoggerExtensions
    {
        static readonly EventId EmptyEventId = new EventId();

        /// <summary>
        /// Log to Splunk.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="logLevel">Log level.</param>
        /// <param name="workflowType">Workflow type.</param>
        /// <param name="workflowInstance">Workflow instance.</param>
        /// <param name="account">Account.</param>
        /// <param name="exception">Exception.</param>
        /// <param name="extraParameters">Extra parameters.</param>
        public static void DefineVTEXLog(this ILogger logger, LogLevel logLevel, string workflowType, string workflowInstance, string account = "", Exception exception = null, params Tuple<string, string>[] extraParameters)
        {
            string formattedMessage = string.Empty;
            logger.Log(logLevel,
                        EmptyEventId,
                        new VTEXSplunkEntry(workflowType, workflowInstance, account, exception, extraParameters),
                        exception, (VTEXSplunkEntry arg1, Exception arg2) =>
                        {
                            if (string.IsNullOrWhiteSpace(formattedMessage))
                            {
                                var eventSegment = $"Event '{workflowType}' on {workflowInstance}";
                                var exceptionSegment = "";
                                if (exception != null)
                                    exceptionSegment = $"Exception type: {exception.GetType().FullName}. Exception message: {exception.Message}";
                                var accountSegment = !string.IsNullOrWhiteSpace(account) ? account : "-";
                                formattedMessage = string.Format($"[{logLevel}] {eventSegment} {accountSegment}. {exceptionSegment}");
                            }
                            return formattedMessage;
                        });
        }
    }
}