using System;
using System.Collections.Generic;

namespace Vtex.SplunkLogger
{
    /// <summary>
    /// This class represents a VTEX log entry.
    /// </summary>
    public class VTEXLogEntry
    {
        /// <summary>
        /// Workflow type is a VTEX concept almost similar as `{EventId.Name}` concept.
        /// </summary>
        public string WorkflowType { get; private set; }

        /// <summary>
        /// Workflow instace is a VTEX concept almost similar as `{EventId.Id}` concept.
        /// </summary>
        public string WorkflowInstance { get; private set; }

        /// <summary>
        /// For which account this log happened.
        /// </summary>
        public string Account { get; private set; }

        /// <summary>
        /// Exception to be logged.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Extra parameters that will be represented as `{Key}="{Value}"` entries at Splunk text message.
        /// </summary>
        public List<Tuple<string, string>> ExtraParameters { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:VTEX.SampleWebAPI.Logging.VTEXLogEntry"/> class.
        /// </summary>
        /// <param name="workflowType">Workflow type.</param>
        /// <param name="workflowInstance">Workflow instance.</param>
        /// <param name="account">Account.</param>
        /// <param name="exception">Exception.</param>
        /// <param name="extraParameters">Extra parameters.</param>
        public VTEXLogEntry(string workflowType, string workflowInstance, string account = "", Exception exception = null, params Tuple<string, string>[] extraParameters)
        {
            if (string.IsNullOrWhiteSpace(workflowType))
                throw new ArgumentNullException(nameof(workflowType));

            if (string.IsNullOrWhiteSpace(workflowInstance))
                throw new ArgumentNullException(nameof(workflowInstance));

            ExtraParameters = new List<Tuple<string, string>>(extraParameters);
            WorkflowType = workflowType;
            WorkflowInstance = workflowInstance;
            Account = account;
            Exception = exception;

            if (exception != null)
            {
                ExtraParameters.Add(new Tuple<string, string>("exception_type", exception.GetType().FullName));
                ExtraParameters.Add(new Tuple<string, string>("exception_message", exception.Message));
            }
        }
    }
}