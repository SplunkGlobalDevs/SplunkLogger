using System;
using System.Collections.Generic;

namespace VTEX.SampleWebAPI.Logging
{
    public class VTEXSplunkEntry
    {
        public string WorkflowType { get; private set; }
        public string WorkflowInstance { get; private set; }
        public string Account { get; private set; }
        public Exception Exception { get; private set; }
        public List<Tuple<string, string>> ExtraParameters { get; private set; }

        public VTEXSplunkEntry(string workflowType, string workflowInstance, string account = "", Exception exception = null, params Tuple<string, string>[] extraParameters)
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