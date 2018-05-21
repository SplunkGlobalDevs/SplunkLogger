using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Splunk.Configurations;
using System.Linq;

namespace Splunk.Providers
{
    /// <summary>
    /// Splunk HECB ase provider.
    /// </summary>
    public abstract class SplunkHECBaseProvider : ILoggerProvider
    {
        protected ILogger loggerInstance;
        protected HttpClient httpClient;

        public SplunkHECBaseProvider(SplunkLoggerConfiguration configuration, string endPointCustomization, Dictionary<string, string> customHeaders = null)
        {
            SetupHttpClient(configuration, endPointCustomization, customHeaders);
        }

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

        void SetupHttpClient(SplunkLoggerConfiguration configuration, string endPointCustomization, Dictionary<string, string> customHeaders)
        {
            httpClient = new HttpClient
            {
                BaseAddress = GetSplunkCollectorUrl(configuration, endPointCustomization)
            };

            if (configuration.HecConfiguration.DefaultTimeoutInMilliseconds > 0)
                httpClient.Timeout = TimeSpan.FromMilliseconds(configuration.HecConfiguration.DefaultTimeoutInMilliseconds);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Splunk", configuration.HecConfiguration.Token);
            if (configuration.HecConfiguration.ChannelIdType == HECConfiguration.ChannelIdOption.RequestHeader)
                httpClient.DefaultRequestHeaders.Add("x-splunk-request-channel", Guid.NewGuid().ToString());

            if(customHeaders != null && customHeaders.Count > 0)
            {
                customHeaders.ToList().ForEach(keyValuePair => httpClient.DefaultRequestHeaders.Add(keyValuePair.Key, keyValuePair.Value));
            }
        }

        protected void DebugSplunkResponse(Task<HttpResponseMessage> responseMessageTask, string loggerType)
        {
            if (responseMessageTask.IsCompletedSuccessfully)
            {
                switch (responseMessageTask.Result.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Request completed successfully.");
                        break;
                    case System.Net.HttpStatusCode.Created:
                        Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Create request completed successfully.");
                        break;
                    case System.Net.HttpStatusCode.BadRequest:
                        Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Request error. See response body for details.");
                        break;
                    case System.Net.HttpStatusCode.Unauthorized:
                        Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Authentication failure, invalid access credentials.");
                        break;
                    case System.Net.HttpStatusCode.PaymentRequired:
                        Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: In-use Splunk Enterprise license disables this feature.");
                        break;
                    case System.Net.HttpStatusCode.Forbidden:
                        Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Insufficient permission.");
                        break;
                    case System.Net.HttpStatusCode.NotFound:
                        Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Requested endpoint does not exist.");
                        break;
                    case System.Net.HttpStatusCode.Conflict:
                        Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Invalid operation for this endpoint. See response body for details.");
                        break;
                    case System.Net.HttpStatusCode.InternalServerError:
                        Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Unspecified internal server error. See response body for details.");
                        break;
                    case System.Net.HttpStatusCode.ServiceUnavailable:
                        Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Feature is disabled in configuration file.");
                        break;
                }
            }
            else if (responseMessageTask.IsCanceled)
                Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Canceled");
            else
                Debug.WriteLine($"[DEBUG] Splunk HEC {loggerType} Status: Error " + responseMessageTask.Exception != null ? responseMessageTask.Exception.ToString() : "");
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