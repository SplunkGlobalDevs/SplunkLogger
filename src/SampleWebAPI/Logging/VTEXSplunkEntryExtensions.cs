using System;
using Microsoft.Extensions.Logging;
using VTEX.SampleWebAPI.Logging;

namespace VTEX.SampleWebAPI
{
    public static class VTEXSplunkEntryExtensions
    {
        static readonly EventId EmptyEventId = new EventId();

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
