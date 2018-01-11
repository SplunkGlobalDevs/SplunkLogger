using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Vtex.SplunkLogger;

namespace Vtex
{
    /// <summary>
    /// This class contains ILogger extension method to simplify the process to record VTEX Logs and Kpis.
    /// </summary>
    public static class ILoggerExtensions
    {
        static readonly EventId emptyEventId = new EventId();
        static readonly ConcurrentDictionary<ILogger, MetricManager> metricManagers = new ConcurrentDictionary<ILogger, MetricManager>();

        static void KpiReady(object sender, VTEXKpiEntry kpiEntry)
        {
            if(sender is ILogger)
                ((ILogger)sender).Log(LogLevel.Critical, emptyEventId, kpiEntry, null, null);
        }

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
                        emptyEventId,
                        new VTEXLogEntry(workflowType, workflowInstance, account, exception, extraParameters),
                        exception, (VTEXLogEntry arg1, Exception arg2) =>
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

        /// <summary>
        /// Generate performance indicator.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="kpiName">Kpi name.</param>
        /// <param name="kpiValue">Kpi value.</param>
        /// <param name="account">Account.</param>
        /// <param name="extraParameters">Extra parameters.</param>
        public static void DefineVTEXKpi(this ILogger logger, string kpiName, float kpiValue, string account = "", params Tuple<string, string>[] extraParameters)
        {
            metricManagers.GetOrAdd(logger, new MetricManager(logger, KpiReady)).RegisterKpi(kpiName, kpiValue, account, extraParameters);
        }
    }
}