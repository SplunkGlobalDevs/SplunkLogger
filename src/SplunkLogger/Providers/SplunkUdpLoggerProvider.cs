using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Splunk.Configurations;
using Splunk.Loggers;
using System.Net.Sockets;

namespace Splunk.Providers
{
    public class SplunkUdpLoggerProvider : ILoggerProvider
    {
        readonly LogLevel threshold;
        readonly ILoggerFormatter loggerFormatter;
        readonly ConcurrentDictionary<string, ILogger> loggers;
        readonly UdpClient udpClient;

        public SplunkUdpLoggerProvider(SplunkLoggerConfiguration configuration, ILoggerFormatter loggerFormatter = null)
        {
            loggers = new ConcurrentDictionary<string, ILogger>();

            threshold = configuration.Threshold;

            this.loggerFormatter = loggerFormatter;

            udpClient = new UdpClient(configuration.SocketConfiguration.HostName, configuration.SocketConfiguration.Port);
            if (!udpClient.Client.Connected)
                udpClient.Client.Connect(configuration.SocketConfiguration.HostName, configuration.SocketConfiguration.Port);
        }

        ILogger CreateLoggerInstance(string categoryName)
        {
            return new UdpLogger(categoryName, threshold, udpClient, loggerFormatter);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, CreateLoggerInstance(categoryName));
        }

        public void Dispose()
        {
            loggers.Clear();
        }
    }
}