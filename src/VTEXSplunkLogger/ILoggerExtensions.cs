using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
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
        static readonly HttpClient evidenceHttpClient = CreateEvidenceHttpClient();

        static string Application = "";

        static HttpClient CreateEvidenceHttpClient()
        {
            HttpClient httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://evidence.vtex.com"), // This is a internal VTEX route. If you aren't from VTEX you will not be able to use it.
                MaxResponseContentBufferSize = 2147483647,
                Timeout = TimeSpan.FromSeconds(10)
            };
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }

        static void KpiReady(object sender, VTEXKpiEntry kpiEntry)
        {
            if(sender is ILogger)
                ((ILogger)sender).Log(LogLevel.Critical, emptyEventId, kpiEntry, null, null);
        }

        static string GenerateHash(string evidenceData)
        {
            var bytes = Encoding.UTF8.GetBytes(evidenceData);
            byte[] md5Bytes = new byte[] { };
            using (var md5Instance = MD5.Create())
            {
                md5Bytes = md5Instance.ComputeHash(bytes);
            }
            return md5Bytes.ToHex();
        }


        /// <summary>
        /// Method used to define application name at project startup.
        /// </summary>
        public static void SetApplication(string application)
        {
            if (string.IsNullOrWhiteSpace(application))
                throw new ArgumentNullException(nameof(application));

            Application = application;
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
            if (string.IsNullOrWhiteSpace(Application))
                throw new NullReferenceException("You must call `ILoggerExtensions.SetApplication` method before call to save log.");

            string formattedMessage = string.Empty;
            string evidenceHash = "";
            string evidenceText = "";

            if (exception != null)
            {
                evidenceText = string.Format(
                    "Account: {1}{0}WorkflowType: {2}{0}WorkflowInstance: {3}{0}ExceptionType: {4}{0}ExceptionMessage: {5}{0}ExceptionBaseStack:{6}{0}ExceptionStack: {7}",
                    Environment.NewLine,
                    account,
                    workflowType,
                    workflowInstance,
                    exception.GetType().Name,
                    exception.GetBaseException().Message,
                    exception.GetBaseException().StackTrace,
                    exception.StackTrace);
                evidenceHash = GenerateHash(evidenceText);
                var putPath = $"/api/evidence?application={Application}&hash={evidenceHash}";
                evidenceHttpClient.PutAsync(putPath, new StringContent(evidenceText, Encoding.UTF8, "text/plain"))
                                  .ContinueWith(task => {
                                      if (task.IsCompletedSuccessfully)
                                          Debug.WriteLine("VTEX Evidence Status: Sucess");
                                      else if (task.IsCanceled)
                                          Debug.WriteLine("VTEX Evidence Status: Canceled");
                                      else
                                          Debug.WriteLine("VTEX Evidence Status: Error " + task.Exception != null ? task.Exception.ToString() : "");
                                  });;
            }
                
            logger.Log(logLevel,
                        emptyEventId,
                       new VTEXLogEntry(Application, workflowType, workflowInstance, account, evidenceHash, extraParameters),
                        exception, (VTEXLogEntry arg1, Exception arg2) =>
                        {
                            if (string.IsNullOrWhiteSpace(formattedMessage))
                            {
                                var eventSegment = $"Event '{workflowType}' on {workflowInstance}";
                                var exceptionSegment = "";
                                if (exception != null)
                                    exceptionSegment = $"Exception type: {exception.GetType().FullName}. Exception message: {exception.Message}";
                                var accountSegment = !string.IsNullOrWhiteSpace(account) ? account : "-";
                                var evidenceSegment = !string.IsNullOrWhiteSpace(evidenceText) ? $"Evidence hash: {evidenceHash}. Evidence text: {evidenceText}" : "";
                                formattedMessage = string.Format($"[{logLevel}] {eventSegment} {accountSegment}.{Environment.NewLine}{exceptionSegment}.{Environment.NewLine}{evidenceSegment}");
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
            if (string.IsNullOrWhiteSpace(Application))
                throw new NullReferenceException("You must call `ILoggerExtensions.SetApplication` method before call to save log.");
            
            metricManagers.GetOrAdd(logger, new MetricManager(logger, KpiReady)).RegisterKpi(Application, kpiName, kpiValue, account, extraParameters);
        }
    }
}