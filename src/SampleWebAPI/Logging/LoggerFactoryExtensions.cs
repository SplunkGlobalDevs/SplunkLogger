using Microsoft.Extensions.Logging;
using Splunk.Providers;
using Splunk.Configurations;
using Splunk;

namespace VTEX.SampleWebAPI
{
    public static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddHECRawSplunkLogger(this ILoggerFactory loggerFactory, SplunkLoggerConfiguration configuration, ILoggerFormatter formatter)
        {
            loggerFactory.AddProvider(new SplunkHECRawLoggerProvider(configuration, formatter));
            return loggerFactory;
        }

        public static ILoggerFactory AddHECJsonSplunkLogger(this ILoggerFactory loggerFactory, SplunkLoggerConfiguration configuration, ILoggerFormatter formatter)
        {
            loggerFactory.AddProvider(new SplunkHECJsonLoggerProvider(configuration, formatter));
            return loggerFactory;
        }

        public static ILoggerFactory AddTcpSplunkLogger(this ILoggerFactory loggerFactory, SplunkLoggerConfiguration configuration, ILoggerFormatter formatter)
        {
            loggerFactory.AddProvider(new SplunkTcpLoggerProvider(configuration, formatter));
            return loggerFactory;
        }

        public static ILoggerFactory AddUdpSplunkLogger(this ILoggerFactory loggerFactory, SplunkLoggerConfiguration configuration, ILoggerFormatter formatter)
        {
            loggerFactory.AddProvider(new SplunkUdpLoggerProvider(configuration, formatter));
            return loggerFactory;
        }
    }
}