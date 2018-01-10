
namespace Splunk.Configurations
{
    /// <summary>
    /// Class used to define configuration for Splunk Socket logger.
    /// </summary>
    public class SocketConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the host.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        public int Port { get; set; }
    }
}