using Microsoft.Extensions.Logging;

namespace Splunk.Configurations
{
    /// <summary>
    /// Class used to define configuration for Splunk logger.
    /// </summary>
    public class SplunkLoggerConfiguration
    {
        /// <summary>
        /// Gets or sets the hec configuration.
        /// </summary>
        /// <value>The hec configuration.</value>
        public HECConfiguration HecConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the socket configuration.
        /// </summary>
        /// <value>The socket configuration.</value>
        public SocketConfiguration SocketConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the threshold.
        /// </summary>
        /// <value>The threshold.</value>
        public LogLevel Threshold { get; set; } = LogLevel.Warning;
    }
}