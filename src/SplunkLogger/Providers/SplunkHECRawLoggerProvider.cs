using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Splunk.Configurations;
using Splunk.Loggers;

namespace Splunk.Providers
{
    /// <summary>
    /// This class is used to provide a Splunk HEC Raw logger for each categoryName.
    /// </summary>
    public class SplunkHECRawLoggerProvider : SplunkHECBaseProvider, ILoggerProvider
    {
        readonly BatchManager batchManager;
        readonly LogLevel threshold;
        readonly ILoggerFormatter loggerFormatter;
        readonly ConcurrentDictionary<string, ILogger> loggers;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.Providers.SplunkHECRawLoggerProvider"/> class.
        /// </summary>
        /// <param name="configuration">Splunk configuration instance for HEC.</param>
        /// <param name="loggerFormatter">Formatter instance.</param>
        public SplunkHECRawLoggerProvider(SplunkLoggerConfiguration configuration, ILoggerFormatter loggerFormatter = null)
        {
            loggers = new ConcurrentDictionary<string, ILogger>();

            threshold = configuration.Threshold;

            this.loggerFormatter = loggerFormatter;

            SetupHttpClient(configuration, "raw");

            batchManager = new BatchManager(configuration.HecConfiguration.BatchSizeCount, configuration.HecConfiguration.BatchIntervalInMilliseconds, Emit);
        }

        /// <summary>
        /// Create a <see cref="T:Splunk.Loggers.HECRawLogger"/> instance to the category name provided.
        /// </summary>
        /// <returns><see cref="T:Splunk.Loggers.HECRawLogger"/> instance.</returns>
        /// <param name="categoryName">Category name.</param>
        public override ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, CreateLoggerInstance(categoryName));
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:Splunk.Providers.SplunkHECRawLoggerProvider"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:Splunk.Providers.SplunkHECRawLoggerProvider"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:Splunk.Providers.SplunkHECRawLoggerProvider"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:Splunk.Providers.SplunkHECRawLoggerProvider"/> so the garbage collector can reclaim the memory
        /// that the <see cref="T:Splunk.Providers.SplunkHECRawLoggerProvider"/> was occupying.</remarks>
        public override void Dispose()
        {
            loggers.Clear();
        }

        /// <summary>
        /// Create a <see cref="T:Splunk.Loggers.HECRawLogger"/> instance to the category name provided.
        /// </summary>
        /// <returns><see cref="T:Splunk.Loggers.HECRawLogger"/> instance.</returns>
        /// <param name="categoryName">Category name.</param>
        public override ILogger CreateLoggerInstance(string categoryName)
        {
            return new HECRawLogger(categoryName, threshold, httpClient, batchManager, loggerFormatter);
        }

        /// <summary>
        /// Method used to emit batched events.
        /// </summary>
        /// <param name="events">Events batched.</param>
        public void Emit(List<object> events)
        {
            var formatedMessage = string.Join(Environment.NewLine, events.Select(evt => evt.ToString()));
            var stringContent = new StringContent(formatedMessage);
            httpClient.PostAsync(string.Empty, stringContent)
                      .ContinueWith(task => {
                          if (task.IsCompletedSuccessfully)
                              switch (task.Result.StatusCode)
                              {
                                  case System.Net.HttpStatusCode.OK:
                                      Debug.WriteLine("Splunk HEC RAW Status: Request completed successfully.");
                                      break;
                                  case System.Net.HttpStatusCode.Created:
                                      Debug.WriteLine("Splunk HEC RAW Status: Create request completed successfully.");
                                      break;
                                  case System.Net.HttpStatusCode.BadRequest:
                                      Debug.WriteLine("Splunk HEC RAW Status: Request error. See response body for details.");
                                      break;
                                  case System.Net.HttpStatusCode.Unauthorized:
                                      Debug.WriteLine("Splunk HEC RAW Status: Authentication failure, invalid access credentials.");
                                      break;
                                  case System.Net.HttpStatusCode.PaymentRequired:
                                      Debug.WriteLine("Splunk HEC RAW Status: In-use Splunk Enterprise license disables this feature.");
                                      break;
                                  case System.Net.HttpStatusCode.Forbidden:
                                      Debug.WriteLine("Splunk HEC RAW Status: Insufficient permission.");
                                      break;
                                  case System.Net.HttpStatusCode.NotFound:
                                      Debug.WriteLine("Splunk HEC RAW Status: Requested endpoint does not exist.");
                                      break;
                                  case System.Net.HttpStatusCode.Conflict:
                                      Debug.WriteLine("Splunk HEC RAW Status: Invalid operation for this endpoint. See response body for details.");
                                      break;
                                  case System.Net.HttpStatusCode.InternalServerError:
                                      Debug.WriteLine("Splunk HEC RAW Status: Unspecified internal server error. See response body for details.");
                                      break;
                                  case System.Net.HttpStatusCode.ServiceUnavailable:
                                      Debug.WriteLine("Splunk HEC RAW Status: Feature is disabled in configuration file.");
                                      break;
                                  default:
                                      break;
                              }
                          else if (task.IsCanceled)
                              Debug.WriteLine("Splunk HEC RAW Status: Canceled");
                          else
                              Debug.WriteLine("Splunk HEC RAW Status: Error " + task.Exception != null ? task.Exception.ToString() : "");
                      });
        }
    }
}