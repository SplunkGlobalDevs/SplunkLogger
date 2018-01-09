using System;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Splunk.Loggers
{
    public class UdpLogger : BaseLogger, ILogger
    {
        readonly UdpClient udpClient;

        public UdpLogger(string categoryName, LogLevel threshold, UdpClient udpClient, ILoggerFormatter loggerFormatter)
            : base(categoryName, threshold, loggerFormatter)
        {
            this.udpClient = udpClient;
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
                udpClient.Send(data, data.Length);
            }
        }
    }
}