using System;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Splunk.Loggers
{
    /// <summary>
    /// Class used to send log to splunk via udp.
    /// </summary>
    public class UdpLogger : BaseLogger, ILogger
    {
        readonly UdpClient udpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.Loggers.UdpLogger"/> class.
        /// </summary>
        /// <param name="categoryName">Category name.</param>
        /// <param name="udpClient">UDP client.</param>
        /// <param name="loggerFormatter">Formatter instance.</param>
        public UdpLogger(string categoryName, UdpClient udpClient, ILoggerFormatter loggerFormatter)
            : base(categoryName, loggerFormatter)
        {
            this.udpClient = udpClient;
        }

        /// <summary>
        /// Method used to create log.
        /// </summary>
        /// <returns>The log.</returns>
        /// <param name="logLevel">Log level.</param>
        /// <param name="eventId">Log event identifier.</param>
        /// <param name="state">Log object state.</param>
        /// <param name="exception">Log Exception.</param>
        /// <param name="formatter">Log text formatter function.</param>
        public override void Log<T>(LogLevel logLevel, EventId eventId, T state, Exception exception, Func<T, Exception, string> formatter)
        {
            string formatedMessage = string.Empty;
            if (loggerFormatter != null)
                formatedMessage = loggerFormatter.Format(categoryName, logLevel, eventId, state, exception);
            else if (formatter != null)
                formatedMessage = formatter(state, exception);

            if (!string.IsNullOrWhiteSpace(formatedMessage))
            {
                formatedMessage = formatedMessage + Environment.NewLine;
                Byte[] data = Encoding.ASCII.GetBytes(formatedMessage);
                udpClient.Send(data, data.Length);
            }
        }
    }
}