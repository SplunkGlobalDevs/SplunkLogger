using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Splunk.Configurations;

namespace Splunk.Providers
{
    /// <summary>
    /// Splunk HECB ase provider.
    /// </summary>
    public abstract class SplunkHECBaseProvider : ILoggerProvider
    {
        protected HttpClient httpClient;

        /// <summary>
        /// Get a <see cref="T:Splunk.Loggers.HECJsonLogger"/> instance to the category name provided.
        /// </summary>
        /// <returns><see cref="T:Splunk.Loggers.HECJsonLogger"/> instance.</returns>
        /// <param name="categoryName">Category name.</param>
        public abstract ILogger CreateLogger(string categoryName);

        /// <summary>
        /// Releases all resource used by the <see cref="T:Splunk.Providers.SplunkHECJsonLoggerProvider"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:Splunk.Providers.SplunkHECJsonLoggerProvider"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:Splunk.Providers.SplunkHECJsonLoggerProvider"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:Splunk.Providers.SplunkHECJsonLoggerProvider"/> so the garbage collector can reclaim the memory
        /// that the <see cref="T:Splunk.Providers.SplunkHECJsonLoggerProvider"/> was occupying.</remarks>
        public abstract void Dispose();

        /// <summary>
        /// Create a <see cref="T:Splunk.Loggers.HECJsonLogger"/> instance to the category name provided.
        /// </summary>
        /// <returns>The logger instance.</returns>
        /// <param name="categoryName">Category name.</param>
        public abstract ILogger CreateLoggerInstance(string categoryName);

        protected void SetupHttpClient(SplunkLoggerConfiguration configuration, string endPointCustomization)
        {
            httpClient = new HttpClient
            {
                BaseAddress = GetSplunkCollectorUrl(configuration, endPointCustomization)
            };

            if (configuration.HecConfiguration.DefaultTimeoutInMiliseconds > 0)
                httpClient.Timeout = TimeSpan.FromMilliseconds(configuration.HecConfiguration.DefaultTimeoutInMiliseconds);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Splunk", configuration.HecConfiguration.Token);
            if (configuration.HecConfiguration.ChannelIdType == HECConfiguration.ChannelIdOption.RequestHeader)
                httpClient.DefaultRequestHeaders.Add("x-splunk-request-channel", Guid.NewGuid().ToString());
        }

        Uri GetSplunkCollectorUrl(SplunkLoggerConfiguration configuration, string endPointCustomization)
        {
            var splunkCollectorUrl = configuration.HecConfiguration.SplunkCollectorUrl;
            if (!splunkCollectorUrl.EndsWith("/", StringComparison.InvariantCulture))
                splunkCollectorUrl = splunkCollectorUrl + "/";

            if(!string.IsNullOrWhiteSpace(endPointCustomization))
                splunkCollectorUrl = splunkCollectorUrl + endPointCustomization;

            if (configuration.HecConfiguration.ChannelIdType == HECConfiguration.ChannelIdOption.QueryString)
                splunkCollectorUrl = splunkCollectorUrl + "?channel=" + Guid.NewGuid().ToString();

            if(configuration.HecConfiguration.UseAuthTokenAsQueryString)
            {
                var tokenParameter = "token=" + configuration.HecConfiguration.Token;
                splunkCollectorUrl = string.Format("{0}{1}{2}", splunkCollectorUrl, splunkCollectorUrl.Contains("?") ? "&" : "?", tokenParameter);
            }

            return new Uri(splunkCollectorUrl);
        }
    }
}