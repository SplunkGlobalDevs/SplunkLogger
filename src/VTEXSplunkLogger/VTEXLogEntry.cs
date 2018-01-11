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
        /// Application name.
        /// </summary>
        public string Application { get; private set; }

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
        /// Extra parameters that will be represented as `{Key}="{Value}"` entries at Splunk text message.
        /// </summary>
        public List<Tuple<string, string>> ExtraParameters { get; private set; }

        /// <summary>
        /// Evidence information.
        /// </summary>
        public string Evidence { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:VTEX.SampleWebAPI.Logging.VTEXLogEntry"/> class.
        /// </summary>
        /// <param name="application">Application name.</param>
        /// <param name="workflowType">Workflow type.</param>
        /// <param name="workflowInstance">Workflow instance.</param>
        /// <param name="account">Account.</param>
        /// <param name="evidence">Evidence text.</param>
        /// <param name="extraParameters">Extra parameters.</param>
        public VTEXLogEntry(string application, string workflowType, string workflowInstance, string account = "", string evidence = "", params Tuple<string, string>[] extraParameters)
        {
            if (string.IsNullOrWhiteSpace(application))
                throw new ArgumentNullException(nameof(application));
            
            if (string.IsNullOrWhiteSpace(workflowType))
                throw new ArgumentNullException(nameof(workflowType));

            if (string.IsNullOrWhiteSpace(workflowInstance))
                throw new ArgumentNullException(nameof(workflowInstance));

            ExtraParameters = new List<Tuple<string, string>>(extraParameters);
            Application = application;
            WorkflowType = workflowType;
            WorkflowInstance = workflowInstance;
            Account = account;
            Evidence = evidence;
        }
    }
}