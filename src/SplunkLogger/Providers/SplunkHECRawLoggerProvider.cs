using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Splunk.Configurations;
using Splunk.Loggers;

namespace Splunk.Providers
{
    /// <summary>
    /// This class is used to provide a Splunk HEC Raw logger for each categoryName.
    /// </summary>
    public class SplunkHECRawLoggerProvider : ILoggerProvider
    {
        readonly BatchManager batchManager;
        readonly HttpClient httpClient;
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

            httpClient = new HttpClient();

            var splunkCollectorUrl = configuration.HecConfiguration.SplunkCollectorUrl;
            if (!splunkCollectorUrl.EndsWith("/", StringComparison.InvariantCulture))
                splunkCollectorUrl += "/";
            
            if(configuration.HecConfiguration.ChannelIdType == HECConfiguration.ChannelIdOption.QueryString)
                splunkCollectorUrl = splunkCollectorUrl + "raw?channel=" + Guid.NewGuid().ToString();
            
            httpClient.BaseAddress = new Uri(splunkCollectorUrl);

            httpClient.Timeout = TimeSpan.FromMilliseconds(configuration.HecConfiguration.DefaultTimeoutInMiliseconds);

            if(!configuration.HecConfiguration.UseTokenAsQueryString)
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Splunk", configuration.HecConfiguration.Token);
            if (configuration.HecConfiguration.ChannelIdType == HECConfiguration.ChannelIdOption.RequestHeader)
                httpClient.DefaultRequestHeaders.Add("x-splunk-request-channel", Guid.NewGuid().ToString());

            batchManager = new BatchManager(configuration.HecConfiguration.BatchSizeCount, configuration.HecConfiguration.BatchIntervalInMiliseconds, Emit);
        }

        ILogger CreateLoggerInstance(string categoryName)
        {
            return new HECRawLogger(categoryName, threshold, httpClient, batchManager, loggerFormatter);
        }

        /// <summary>
        /// Create a <see cref="T:Splunk.Loggers.HECRawLogger"/> instance to the category name provided.
        /// </summary>
        /// <returns><see cref="T:Splunk.Loggers.HECRawLogger"/> instance.</returns>
        /// <param name="categoryName">Category name.</param>
        public ILogger CreateLogger(string categoryName)
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
            var formatedMessage = string.Join(Environment.NewLine, events.Select(evt => evt.ToString()));
            var stringContent = new StringContent(formatedMessage);
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