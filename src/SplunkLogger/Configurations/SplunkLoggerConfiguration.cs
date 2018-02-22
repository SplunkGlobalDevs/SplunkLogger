
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
        public HECConfiguration HecConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the socket configuration.
        /// </summary>
        public SocketConfiguration SocketConfiguration { get; set; }
    }
}