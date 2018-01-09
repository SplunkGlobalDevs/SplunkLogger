using System;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text;

namespace Splunk.Loggers
{
    public class TcpLogger : BaseLogger, ILogger
    {
        readonly TcpClient tcpClient;

        public TcpLogger(string categoryName, LogLevel threshold, TcpClient tcpClient, ILoggerFormatter loggerFormatter)
            : base(categoryName, threshold, loggerFormatter)
        {
            this.tcpClient = tcpClient;
        }

        public void Log<T>(LogLevel logLevel, EventId eventId, T state, Exception exception, Func<T, Exception, string> formatter)
        {
            string formatedMessage = string.Empty;
            if (loggerFormatter != null)
                formatedMessage = loggerFormatter.Format(logLevel, eventId, state, exception);
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