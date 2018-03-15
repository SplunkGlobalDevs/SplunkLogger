using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        readonly ILoggerFormatter loggerFormatter;
        readonly ConcurrentDictionary<string, ILogger> loggers;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.Providers.SplunkHECRawLoggerProvider"/> class.
        /// </summary>
        /// <param name="configuration">Splunk configuration instance for HEC.</param>
        /// <param name="loggerFormatter">Formatter instance.</param>
        public SplunkHECRawLoggerProvider(SplunkLoggerConfiguration configuration, ILoggerFormatter loggerFormatter = null)
            :base(configuration, "raw")
        {
            this.loggerFormatter = loggerFormatter;
            loggers = new ConcurrentDictionary<string, ILogger>();
            batchManager = new BatchManager(configuration.HecConfiguration.BatchSizeCount, configuration.HecConfiguration.BatchIntervalInMilliseconds, Emit);
        }

        /// <summary>
        /// Get a <see cref="T:Splunk.Loggers.HECJsonLogger"/> instance to the category name provided.
        /// </summary>
        /// <returns><see cref="T:Splunk.Loggers.HECJsonLogger"/> instance.</returns>
        /// <param name="categoryName">Category name.</param>
        public override ILogger CreateLogger(string categoryName)
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
        public override void Dispose()
        {
            loggers.Clear();
        }

        /// <summary>
        /// Method used to emit batched events.
        /// </summary>
        /// <param name="events">Events batched.</param>
        public void Emit(List<object> events)
        {
            var formatedMessage = string.Join("\\r\\n", events.Select(evt => evt.ToString().Trim()));
            var stringContent = new StringContent(formatedMessage);
            httpClient.PostAsync(string.Empty, stringContent)
                      .ContinueWith(task => DebugSplunkResponse(task, "raw"));
        }

        ILogger CreateLoggerInstance(string categoryName)
        {
            return new HECRawLogger(categoryName, httpClient, batchManager, loggerFormatter);
        }
    }
}