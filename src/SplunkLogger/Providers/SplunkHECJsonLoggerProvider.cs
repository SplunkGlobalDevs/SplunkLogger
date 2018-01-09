using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public class SplunkHECJsonLoggerProvider : ILoggerProvider
    {
        readonly BatchManager batchController;
        readonly HttpClient httpClient;
        readonly LogLevel threshold;
        readonly ILoggerFormatter loggerFormatter;
        readonly ConcurrentDictionary<string, ILogger> loggers;

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

        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, CreateLoggerInstance(categoryName));
        }

        public void Dispose()
        {
            loggers.Clear();
        }

        public void Emit(List<object> events)
        {
            var jArray = events.Select(evt => JsonConvert.SerializeObject(evt, Formatting.None));
            var formatedMessage = string.Join(" ", jArray);
            var stringContent = new StringContent(formatedMessage, Encoding.UTF8, "application/json");
            httpClient.PostAsync(string.Empty, stringContent);
        }
    }
}