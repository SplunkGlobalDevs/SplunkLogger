using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Concurrent;
using Splunk.Configurations;
using Splunk.Loggers;

namespace Splunk.Providers
{
    public class SplunkHECRawLoggerProvider : ILoggerProvider
    {
        readonly BatchManager batchManager;
        readonly HttpClient httpClient;
        readonly LogLevel threshold;
        readonly ILoggerFormatter loggerFormatter;
        readonly ConcurrentDictionary<string, ILogger> loggers;

        public SplunkHECRawLoggerProvider(SplunkLoggerConfiguration configuration, ILoggerFormatter loggerFormatter = null)
        {
            loggers = new ConcurrentDictionary<string, ILogger>();

            threshold = configuration.Threshold;

            this.loggerFormatter = loggerFormatter;

            httpClient = new HttpClient();

            var splunkCollectorUrl = configuration.HecConfiguration.SplunkCollectorUrl;
            if (!splunkCollectorUrl.EndsWith("/", StringComparison.InvariantCulture))
                splunkCollectorUrl += "/";

            var baseAddress = new Uri(splunkCollectorUrl + "/raw?channel=" + Guid.NewGuid().ToString());
            httpClient.BaseAddress = baseAddress;

            httpClient.Timeout = TimeSpan.FromMilliseconds(configuration.HecConfiguration.DefaultTimeoutInMiliseconds);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Splunk", configuration.HecConfiguration.Token);

            batchManager = new BatchManager(configuration.HecConfiguration.BatchSizeCount, configuration.HecConfiguration.BatchIntervalInMiliseconds, Emit);
        }

        ILogger CreateLoggerInstance(string categoryName)
        {
            return new HECRawLogger(categoryName, threshold, httpClient, batchManager, loggerFormatter);
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
            var formatedMessage = string.Join(Environment.NewLine, events.Select(evt => evt.ToString()));
            var stringContent = new StringContent(formatedMessage);
            httpClient.PostAsync(string.Empty, stringContent);
        }
    }
}