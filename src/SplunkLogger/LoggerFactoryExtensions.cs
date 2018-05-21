using Microsoft.Extensions.Logging;
using Splunk.Providers;
using Splunk.Configurations;
using System.Collections.Generic;

namespace Splunk
{
    /// <summary>
    /// This class contains ILoggerFactory extension method to simplify the process to add a Splunk logger provider.
    /// </summary>
    public static class LoggerFactoryExtensions
    {
        static readonly ILoggerFormatter DefaultLoggerFormatter = new BasicLoggerFormatter();

        /// <summary>
        /// Add <see cref="T:Splunk.Providers.SplunkHECRawLoggerProvider"/> as provider to logger factory.
        /// </summary>
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="formatter">Custom text formatter.</param>
        public static ILoggerFactory AddHECRawSplunkLogger(this ILoggerFactory loggerFactory, SplunkLoggerConfiguration configuration, ILoggerFormatter formatter = null)
        {
            if (formatter == null)
                formatter = DefaultLoggerFormatter;
            loggerFactory.AddProvider(new SplunkHECRawLoggerProvider(configuration, formatter));
            return loggerFactory;
        }

        /// <summary>
        /// Add <see cref="T:Splunk.Providers.SplunkHECJsonLoggerProvider"/> as provider to logger factory.
        /// </summary>
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="formatter">Custom text formatter.</param>
        public static ILoggerFactory AddHECJsonSplunkLogger(this ILoggerFactory loggerFactory, SplunkLoggerConfiguration configuration, ILoggerFormatter formatter = null)
        {
            if (formatter == null)
                formatter = DefaultLoggerFormatter;
            loggerFactory.AddProvider(new SplunkHECJsonLoggerProvider(configuration, formatter));
            return loggerFactory;
        }

        /// <summary>
        /// Add <see cref="T:Splunk.Providers.SplunkTcpLoggerProvider"/> as provider to logger factory.
        /// </summary>
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="formatter">Custom text formatter.</param>
        public static ILoggerFactory AddTcpSplunkLogger(this ILoggerFactory loggerFactory, SplunkLoggerConfiguration configuration, ILoggerFormatter formatter = null)
        {
            if (formatter == null)
                formatter = DefaultLoggerFormatter;
            loggerFactory.AddProvider(new SplunkTcpLoggerProvider(configuration, formatter));
            return loggerFactory;
        }

        /// <summary>
        /// Add <see cref="T:Splunk.Providers.SplunkUdpLoggerProvider"/> as provider to logger factory.
        /// </summary>
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="formatter">Custom text formatter.</param>
        public static ILoggerFactory AddUdpSplunkLogger(this ILoggerFactory loggerFactory, SplunkLoggerConfiguration configuration, ILoggerFormatter formatter = null)
        {
            if (formatter == null)
                formatter = DefaultLoggerFormatter;
            loggerFactory.AddProvider(new SplunkUdpLoggerProvider(configuration, formatter));
            return loggerFactory;
        }
    }
}