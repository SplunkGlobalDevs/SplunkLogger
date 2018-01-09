using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Splunk.Configurations;
using Splunk.Loggers;
using System.Net.Sockets;

namespace Splunk.Providers
{
    public class SplunkTcpLoggerProvider : ILoggerProvider
    {
        readonly LogLevel threshold;
        readonly ILoggerFormatter loggerFormatter;
        readonly ConcurrentDictionary<string, ILogger> loggers;
        readonly TcpClient tcpClient;

        public SplunkTcpLoggerProvider(SplunkLoggerConfiguration configuration, ILoggerFormatter loggerFormatter = null)
        {
            loggers = new ConcurrentDictionary<string, ILogger>();

            threshold = configuration.Threshold;

            this.loggerFormatter = loggerFormatter;

            tcpClient = new TcpClient(configuration.SocketConfiguration.HostName, configuration.SocketConfiguration.Port);
            if (!tcpClient.Connected)
                tcpClient.Connect(configuration.SocketConfiguration.HostName, configuration.SocketConfiguration.Port);
        }

        ILogger CreateLoggerInstance(string categoryName)
        {
            return new TcpLogger(categoryName, threshold, tcpClient, loggerFormatter);
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