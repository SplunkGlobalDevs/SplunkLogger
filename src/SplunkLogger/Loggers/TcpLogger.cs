using System;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text;

namespace Splunk.Loggers
{
    /// <summary>
    /// Class used to send log to splunk via tcp.
    /// </summary>
    public class TcpLogger : BaseLogger, ILogger
    {
        readonly TcpClient tcpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.Loggers.TcpLogger"/> class.
        /// </summary>
        /// <param name="categoryName">Category name.</param>
        /// <param name="tcpClient">Tcp client.</param>
        /// <param name="loggerFormatter">Formatter instance.</param>
        public TcpLogger(string categoryName, TcpClient tcpClient, ILoggerFormatter loggerFormatter)
            : base(categoryName, loggerFormatter)
        {
            this.tcpClient = tcpClient;
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
                tcpClient.Client.Send(data);
            }
        }
    }
}