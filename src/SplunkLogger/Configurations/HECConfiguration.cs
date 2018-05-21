
using System.Collections.Generic;

namespace Splunk.Configurations
{
    /// <summary>
    /// Class used to define configuration for Splunk HEC logger.
    /// </summary>
    public class HECConfiguration
    {
        public enum ChannelIdOption
        {
            None,
            QueryString,
            RequestHeader
        }

        /// <summary>
        /// Gets or sets the batch interval in milliseconds.
        /// </summary>
        public int BatchIntervalInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the batch size count.
        /// </summary>
        public uint BatchSizeCount { get; set; }

        /// <summary>
        /// Gets or sets the default timeout in milliseconds.
        /// </summary>
        public int DefaultTimeoutInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the splunk collector URL.
        /// </summary>
        public string SplunkCollectorUrl { get; set; }

        /// <summary>
        /// Gets or sets indication to use or not hec token autentication at query string.
        /// </summary>
        public bool UseAuthTokenAsQueryString { get; set; }

        /// <summary>
        /// Gets or sets indication to use or not channel identification when using raw endpoint.
        /// It's important to say that Splunk documentation indicates that ChannelId parameter is only
        /// required for Splunk versions older than 6.7.0.
        /// So if you have earlier version you can supress send this extra data by using <value>ChannelIdOption.None</value>
        /// </summary>
        public ChannelIdOption ChannelIdType { get; set; } = ChannelIdOption.None;

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or set custom header to be applied at HEC calls.
        /// </summary>
        /// <value>The custom headers.</value>
        public Dictionary<string, string> CustomHeaders { get; set; }
    }
}