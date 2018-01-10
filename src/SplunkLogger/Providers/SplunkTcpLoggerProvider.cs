using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Splunk.Configurations;
using Splunk.Loggers;
using System.Net.Sockets;

namespace Splunk.Providers
{
    /// <summary>
    /// This class is used to provide a Splunk Socket Tcp logger for each categoryName.
    /// </summary>
    public class SplunkTcpLoggerProvider : ILoggerProvider
    {
        readonly LogLevel threshold;
        readonly ILoggerFormatter loggerFormatter;
        readonly ConcurrentDictionary<string, ILogger> loggers;
        readonly TcpClient tcpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.Providers.SplunkTcpLoggerProvider"/> class.
        /// </summary>
        /// <param name="configuration">Splunk configuration instance for Socket.</param>
        /// <param name="loggerFormatter">Formatter instance.</param>
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

        /// <summary>
        /// Create a <see cref="T:Splunk.Loggers.TcpLogger"/> instance to the category name provided.
        /// </summary>
        /// <returns><see cref="T:Splunk.Loggers.TcpLogger"/> instance.</returns>
        /// <param name="categoryName">Category name.</param>
        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, CreateLoggerInstance(categoryName));
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:Splunk.Providers.SplunkTcpLoggerProvider"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:Splunk.Providers.SplunkTcpLoggerProvider"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:Splunk.Providers.SplunkTcpLoggerProvider"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:Splunk.Providers.SplunkTcpLoggerProvider"/> so the garbage collector can reclaim the memory
        /// that the <see cref="T:Splunk.Providers.SplunkTcpLoggerProvider"/> was occupying.</remarks>
        public void Dispose()
        {
            loggers.Clear();
        }
    }
}