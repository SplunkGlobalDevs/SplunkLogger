using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Splunk.Configurations;
using Splunk.Loggers;
using System.Net.Sockets;

namespace Splunk.Providers
{
    /// <summary>
    /// Class used to provide Splunk Socket Udp logger.
    /// </summary>
    public class SplunkUdpLoggerProvider : ILoggerProvider
    {
        readonly LogLevel threshold;
        readonly ILoggerFormatter loggerFormatter;
        readonly ConcurrentDictionary<string, ILogger> loggers;
        readonly UdpClient udpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.Providers.SplunkUdpLoggerProvider"/> class.
        /// </summary>
        /// <param name="configuration">Splunk configuration instance for Socket.</param>
        /// <param name="loggerFormatter">Formatter instance.</param>
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

        /// <summary>
        /// Create a <see cref="T:Splunk.Loggers.UdpLogger"/> instance to the category name provided.
        /// </summary>
        /// <returns><see cref="T:Splunk.Loggers.UdpLogger"/> instance.</returns>
        /// <param name="categoryName">Category name.</param>
        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, CreateLoggerInstance(categoryName));
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:Splunk.Providers.SplunkUdpLoggerProvider"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:Splunk.Providers.SplunkUdpLoggerProvider"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:Splunk.Providers.SplunkUdpLoggerProvider"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:Splunk.Providers.SplunkUdpLoggerProvider"/> so the garbage collector can reclaim the memory
        /// that the <see cref="T:Splunk.Providers.SplunkUdpLoggerProvider"/> was occupying.</remarks>
        public void Dispose()
        {
            loggers.Clear();
        }
    }
}