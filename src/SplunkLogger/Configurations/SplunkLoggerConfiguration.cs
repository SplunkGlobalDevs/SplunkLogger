using Microsoft.Extensions.Logging;

namespace Splunk.Configurations
{
    public class SplunkLoggerConfiguration
    {
        public HECConfiguration HecConfiguration { get; set; }
        public SocketConfiguration SocketConfiguration { get; set; }
        public LogLevel Threshold { get; set; } = LogLevel.Warning;
    }
}