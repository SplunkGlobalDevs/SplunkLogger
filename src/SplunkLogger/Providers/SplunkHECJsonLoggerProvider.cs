using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Splunk.Configurations;
using Splunk.Loggers;

namespace Splunk.Providers
{
    /// <summary>
    /// This class is used to provide a Splunk HEC Json logger for each categoryName.
    /// </summary>
    public class SplunkHECJsonLoggerProvider : ILoggerProvider
    {
        readonly BatchManager batchController;
        readonly HttpClient httpClient;
        readonly LogLevel threshold;
        readonly ILoggerFormatter loggerFormatter;
        readonly ConcurrentDictionary<string, ILogger> loggers;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.Providers.SplunkHECJsonLoggerProvider"/> class.
        /// </summary>
        /// <param name="configuration">Splunk configuration instance for HEC.</param>
        /// <param name="loggerFormatter">Formatter instance.</param>
        public SplunkHECJsonLoggerProvider(SplunkLoggerConfiguration configuration, ILoggerFormatter loggerFormatter = null)
        {
            loggers = new ConcurrentDictionary<string, ILogger>();

            threshold = configuration.Threshold;

            this.loggerFormatter = loggerFormatter;

            httpClient = new HttpClient();

            var splunkCollectorUrl = configuration.HecConfiguration.SplunkCollectorUrl;
            if (!splunkCollectorUrl.EndsWith("/", StringComparison.InvariantCulture))
                splunkCollectorUrl += "/";

            var baseAddress = new Uri(splunkCollectorUrl + "event");
            httpClient.BaseAddress = baseAddress;

            httpClient.Timeout = TimeSpan.FromMilliseconds(configuration.HecConfiguration.DefaultTimeoutInMiliseconds);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Splunk", configuration.HecConfiguration.Token);

            batchController = new BatchManager(configuration.HecConfiguration.BatchSizeCount, configuration.HecConfiguration.BatchIntervalInMiliseconds, Emit);
        }

        ILogger CreateLoggerInstance(string categoryName)
        {
            return new HECJsonLogger(categoryName, threshold, httpClient, batchController, loggerFormatter);
        }

        /// <summary>
        /// Create a <see cref="T:Splunk.Loggers.HECJsonLogger"/> instance to the category name provided.
        /// </summary>
        /// <returns><see cref="T:Splunk.Loggers.HECJsonLogger"/> instance.</returns>
        /// <param name="categoryName">Category name.</param>
        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, CreateLoggerInstance(categoryName));
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:Splunk.Providers.SplunkHECJsonLoggerProvider"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:Splunk.Providers.SplunkHECJsonLoggerProvider"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:Splunk.Providers.SplunkHECJsonLoggerProvider"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:Splunk.Providers.SplunkHECJsonLoggerProvider"/> so the garbage collector can reclaim the memory
        /// that the <see cref="T:Splunk.Providers.SplunkHECJsonLoggerProvider"/> was occupying.</remarks>
        public void Dispose()
        {
            loggers.Clear();
        }

        /// <summary>
        /// Method used to emit batched events.
        /// </summary>
        /// <param name="events">Events batched.</param>
        public void Emit(List<object> events)
        {
            var jArray = events.Select(evt => JsonConvert.SerializeObject(evt, Formatting.None));
            var formatedMessage = string.Join(" ", jArray);
            var stringContent = new StringContent(formatedMessage, Encoding.UTF8, "application/json");
            httpClient.PostAsync(string.Empty, stringContent)
                      .ContinueWith(task => {
                          if (task.IsCompletedSuccessfully)
                              Debug.WriteLine("Splunk HEC RAW Status: Sucess");
                          else if (task.IsCanceled)
                              Debug.WriteLine("Splunk HEC RAW Status: Canceled");
                          else
                              Debug.WriteLine("Splunk HEC RAW Status: Error " + task.Exception != null ? task.Exception.ToString() : "");
                      });
        }
    }
}