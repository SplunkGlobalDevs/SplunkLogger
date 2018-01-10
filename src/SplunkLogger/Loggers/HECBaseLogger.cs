using System.Net.Http;
using Microsoft.Extensions.Logging;
               
namespace Splunk.Loggers
{
    /// <summary>
    /// Define a base HEC logger class.
    /// </summary>
    public abstract class HECBaseLogger : BaseLogger
    {
        protected readonly BatchManager batchManager;

        readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.Loggers.HECBaseLogger"/> class.
        /// </summary>
        /// <param name="categoryName">Category name.</param>
        /// <param name="threshold">Threshold.</param>
        /// <param name="httpClient">Http client.</param>
        /// <param name="batchManager">Batch manager.</param>
        /// <param name="loggerFormatter">Formatter instance.</param>
        public HECBaseLogger(string categoryName, LogLevel threshold, HttpClient httpClient, BatchManager batchManager, ILoggerFormatter loggerFormatter)
            : base(categoryName, threshold, loggerFormatter)
        {
            this.httpClient = httpClient;
            this.batchManager = batchManager;
        }
    }
}